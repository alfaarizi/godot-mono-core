using Godot;
using System;
using System.Collections.Generic;

public partial class PathfindingComponent : Component
{
    [Signal] public delegate void PathCompletedEventHandler();

    [Export] public MovementComponent? MovementComponent { get; set; }
    [Export] public TileMapLayer? TileMapLayer { get; set; }
    [Export] public bool EnableDiagonalMovement { get; set; } = true;
    [Export] public float WaypointDistance { get; set; } = 20.0f;

    private Node2D? _target;
    private AStarGrid2D _grid = new();
    private HashSet<Vector2I> _solidPoints = new();
    private Vector2[] _waypoints = Array.Empty<Vector2>();
    private int _currentWaypointIndex;

    public HashSet<Vector2I> GetSolidPoints()
        => _solidPoints;

    public Vector2[] GetWaypoints()
        => _waypoints;

    public Vector2I ToGridPos(Vector2 worldPosition)
        => TileMapLayer?.LocalToMap(TileMapLayer.ToLocal(worldPosition)) - TileMapLayer?.GetUsedRect().Position ?? Vector2I.Zero;

    public Vector2 ToWorldPos(Vector2I gridPosition)
        => TileMapLayer?.ToGlobal(TileMapLayer.MapToLocal(gridPosition + TileMapLayer.GetUsedRect().Position)) ?? Vector2.Zero;

    public bool IsPathfinding()
        => _waypoints.Length > 0 && _currentWaypointIndex < _waypoints.Length;

    public bool IsPathHalfComplete()
        => _currentWaypointIndex >= (_waypoints.Length / 2);

    public void SetPath(Vector2 endPosition)
    {
        if (!IsEnabled || _target == null || _grid == null) return;
        if (IsPathfinding()) ForceStop();

        var (start, end) = (ToGridPos(_target.GlobalPosition), ToGridPos(endPosition));
        if (!_grid.IsInBoundsv(start) || !_grid.IsInBoundsv(end)) return;

        _waypoints = _grid.GetPointPath(start, end);
        _currentWaypointIndex = 0;
    }

    public void ForceStop()
    {
        _waypoints = Array.Empty<Vector2>();
        _currentWaypointIndex = 0;
        MovementComponent?.SetDirection(Vector2.Zero);
        _ = EmitSignal(SignalName.PathCompleted);
    }

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint())
        {
            SetProcess(false);
            return;
        }

        MovementComponent ??= GetNodeOrNull<MovementComponent>("%MovementComponent");
        _target = MovementComponent?.Body;
        _ = CallDeferred(nameof(SetupGrid));
    }

    public override void _Process(double delta)
    {
        if (!IsEnabled || _waypoints.Length == 0 || MovementComponent == null || _target == null) return;

        if (_currentWaypointIndex >= _waypoints.Length)
        {
            ForceStop();
            return;
        }

        var waypoint = ToWorldPos(new Vector2I((int)_waypoints[_currentWaypointIndex].X, (int)_waypoints[_currentWaypointIndex].Y));

        if (_target.GlobalPosition.DistanceSquaredTo(waypoint) <= WaypointDistance * WaypointDistance)
            _currentWaypointIndex++;
        else
            MovementComponent.SetDirection((waypoint - _target.GlobalPosition).Normalized());
    }

    private async void SetupGrid()
    {
        var currentRoom = Global.GetCurrentRoom();
        if (currentRoom == null) return;

        TileMapLayer = currentRoom.TileMapRect;
        if (TileMapLayer == null) return;

        var navigationData = Global.GetNavigationData(currentRoom.Name);
        if (navigationData != null)
        {
            _grid = navigationData.Grid;
            _solidPoints = navigationData.SolidPoints;
            return;
        }

        _ = await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        var rect = TileMapLayer.GetUsedRect();

        _grid = new AStarGrid2D
        {
            Region = new Rect2I(Vector2I.Zero, rect.Size),
            DefaultComputeHeuristic = EnableDiagonalMovement ? AStarGrid2D.Heuristic.Octile : AStarGrid2D.Heuristic.Manhattan,
            DefaultEstimateHeuristic = EnableDiagonalMovement ? AStarGrid2D.Heuristic.Octile : AStarGrid2D.Heuristic.Manhattan,
            DiagonalMode = EnableDiagonalMovement ? AStarGrid2D.DiagonalModeEnum.Always : AStarGrid2D.DiagonalModeEnum.Never
        };
        _grid.Update();
        _solidPoints = new HashSet<Vector2I>();

        var space = TileMapLayer.GetWorld2D().DirectSpaceState;
        var tileSize = TileMapLayer.TileSet.TileSize.X * 4;
        var query = new PhysicsShapeQueryParameters2D
        {
            CollisionMask = 1,
            Shape = new RectangleShape2D { Size = new Vector2(tileSize, tileSize) }
        };

        for (int x = 0; x < rect.Size.X; x++)
        {
            for (int y = 0; y < rect.Size.Y; y++)
            {
                var gridPos = new Vector2I(x, y);
                if (!_grid.IsInBoundsv(gridPos)) continue;

                query.Transform = new Transform2D(0, ToWorldPos(gridPos));
                if (space.IntersectShape(query).Count > 0)
                {
                    _ = _solidPoints.Add(gridPos);
                    _grid.SetPointSolid(gridPos, true);
                }
            }
        }
        EventBus.EmitNavigationDataAdded(currentRoom.Name, new NavigationData(_grid, _solidPoints));
    }
}