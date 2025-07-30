using Godot;
using System.Runtime.Serialization;

[Tool]
public partial class Brother : Character
{
    [Export] public float MinTargetDistance { get; set; } = 32.0f;
    [Export] public float MinPathProgressDistance { get; set; } = 8.0f;
    [Export] public int PathValidationFrames { get; set; } = 90;

    public MovementComponent? MovementComponent { get; private set; }
    public AnimationComponent? AnimationComponent { get; private set; }
    public PathfindingComponent? PathfindingComponent { get; private set; }

    private LOSManager? _losManager;
    private Vector2 _initialPosition;
    private Vector2 _lastTarget;
    private Vector2 _lastPathValidationPosition;
    private ulong _lastPathValidationFrame;

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint()) { SetProcess(false); return; }

        _ = CallDeferred(nameof(SetInitialPosition));
        MovementComponent ??= GetNodeOrNull<MovementComponent>("%MovementComponent");
        AnimationComponent ??= GetNodeOrNull<AnimationComponent>("%AnimationComponent");
        PathfindingComponent ??= GetNodeOrNull<PathfindingComponent>("%PathfindingComponent");
        _losManager = GetNodeOrNull<LOSManager>("%LOSManager");
    }

    private void SetInitialPosition() => _initialPosition = GlobalPosition;

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint() || MovementComponent == null || PathfindingComponent == null) return;

        var targetPos = GetTargetPosition();
        var targetPosChanged = targetPos.DistanceSquaredTo(_lastTarget) > MinTargetDistance * MinTargetDistance;

        if (targetPosChanged && (!PathfindingComponent.IsPathfinding() || PathfindingComponent.IsPathHalfComplete()))
        {
            PathfindingComponent.SetPath(targetPos);
            _lastTarget = targetPos;
        }

        var currentFrame = Engine.GetProcessFrames();
        if (PathfindingComponent.IsPathfinding() && currentFrame - _lastPathValidationFrame >= (ulong)PathValidationFrames)
        {
            if (MovementComponent.IsColliding() && GlobalPosition.DistanceSquaredTo(_lastPathValidationPosition) < MinPathProgressDistance * MinPathProgressDistance)
                PathfindingComponent.SetPath(_lastTarget);
            _lastPathValidationPosition = GlobalPosition;
            _lastPathValidationFrame = currentFrame;
        }

        if (AnimationComponent != null)
        {
            bool isMoving = MovementComponent.IsMoving();
            Vector2 lastDirection = MovementComponent.GetLastDirection();
            AnimationComponent.SetTreeParameter("conditions/is_moving", isMoving);
            AnimationComponent.SetTreeParameter("conditions/!is_moving", !isMoving);
            AnimationComponent.SetTreeParameter("Move/blend_position", lastDirection);
            AnimationComponent.SetTreeParameter("Idle/blend_position", lastDirection);
        }

        QueueRedraw();
    }

    public override void _Draw()
    {
        if (Engine.IsEditorHint()) return;

        if (_lastTarget != Vector2.Zero)
        {
            Vector2 local = ToLocal(_lastTarget);
            DrawLine(Vector2.Zero, local, Colors.Red, 3.0f);
            DrawCircle(local, 10.0f, Colors.Yellow);
        }

        Vector2 initial = ToLocal(_initialPosition);
        DrawLine(Vector2.Zero, initial, Colors.Blue, 2.0f);
        DrawCircle(initial, 8.0f, Colors.Cyan);

        var p = PathfindingComponent;
        if (p == null) return;

        var solidPoints = p.GetSolidPoints();
        var waypoints = p.GetWaypoints();

        if (solidPoints != null)
        {
            foreach (var gridPos in solidPoints)
                DrawCircle(ToLocal(p.ToWorldPos(gridPos)), 5.0f, Colors.Blue);
        }

        if (waypoints.Length > 1)
        {
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                var startPathPos = ToLocal(p.ToWorldPos(new Vector2I((int)waypoints[i].X, (int)waypoints[i].Y)));
                var nextPathPos = ToLocal(p.ToWorldPos(new Vector2I((int)waypoints[i + 1].X, (int)waypoints[i + 1].Y)));
                DrawLine(startPathPos, nextPathPos, Colors.White, 3.0f);
            }
        }
    }

    private Vector2 GetTargetPosition()
    {
        if (_losManager?.IsTargetVisible(GlobalPosition) == true)
        {
            var (hasLOS, pos) = _losManager.GetNearestLOSToTarget(GlobalPosition, PathfindingComponent?.GetSolidPoints());
            if (hasLOS) return pos;
        }
        return _initialPosition;
    }
}