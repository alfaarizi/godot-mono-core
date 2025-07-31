using Godot;
using System.Runtime.Serialization;

[Tool]
public partial class Brother : Character
{
    [Export] public float MinMovementDistance { get; set; } = 32.0f;
    [Export] public float MinPathProgressDistance { get; set; } = 8.0f;
    [Export] public int PathValidationFrames { get; set; } = 90;

    public MovementComponent? MovementComponent { get; private set; }
    public AnimationComponent? AnimationComponent { get; private set; }
    public PathfindingComponent? PathfindingComponent { get; private set; }

    private CameraComponent? _camera;
    private LOSManager? _losManager;
    private Vector2 _initialPos;
    private Vector2 _lastPos;
    private Vector2 _lastTargetPos = Vector2.Inf;
    private Vector2 _lastLOSPos;
    private Vector2 _lastPathValidationPos;
    private ulong _lastPathValidationFrame;

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint()) { SetProcess(false); return; }

        MovementComponent ??= GetNodeOrNull<MovementComponent>("%MovementComponent");
        AnimationComponent ??= GetNodeOrNull<AnimationComponent>("%AnimationComponent");
        PathfindingComponent ??= GetNodeOrNull<PathfindingComponent>("%PathfindingComponent");
        _losManager = GetNodeOrNull<LOSManager>("%LOSManager");

        _ = CallDeferred(nameof(Setup));
    }

    private void Setup()
    {
        _initialPos = GlobalPosition;
        _camera = Global.GetCurrentCamera();
    }

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint() || MovementComponent == null || PathfindingComponent == null) return;

        var nextPos = GetNextPosition();
        var posChanged = nextPos.DistanceSquaredTo(_lastPos) > MinMovementDistance * MinMovementDistance;

        if (posChanged && (!PathfindingComponent.IsPathfinding() || PathfindingComponent.IsPathHalfComplete()))
        {
            PathfindingComponent.SetPath(nextPos);
            _lastPos = nextPos;
        }

        var currentFrame = Engine.GetProcessFrames();
        if (PathfindingComponent.IsPathfinding() && currentFrame - _lastPathValidationFrame >= (ulong)PathValidationFrames)
        {
            if (MovementComponent.IsColliding() && GlobalPosition.DistanceSquaredTo(_lastPathValidationPos) < MinPathProgressDistance * MinPathProgressDistance)
            {
                if (_losManager != null)
                {
                    var (_, tempPos) = _losManager.GetNearestLOSToTarget();
                    PathfindingComponent.SetPath(tempPos);
                }
            }
            _lastPathValidationPos = GlobalPosition;
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

        if (_lastPos != Vector2.Zero)
        {
            Vector2 local = ToLocal(_lastPos);
            DrawLine(Vector2.Zero, local, Colors.Red, 3.0f);
            DrawCircle(local, 10.0f, Colors.Yellow);
        }

        Vector2 initial = ToLocal(_initialPos);
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

    private Vector2 GetNextPosition()
    {
        if (_losManager == null || _losManager.Target == null || _camera == null) return _initialPos;

        if (!_camera.IsInViewport(GlobalPosition))
            return _initialPos;

        var nextTargetPos = _losManager.Target.GlobalPosition;
        var targetPosChanged = nextTargetPos.DistanceSquaredTo(_lastTargetPos) > MinMovementDistance * MinMovementDistance;
        var tooFarFromLOS = GlobalPosition.DistanceSquaredTo(_lastLOSPos) > MinMovementDistance * MinMovementDistance;

        if (targetPosChanged || tooFarFromLOS)
        {
            _lastTargetPos = nextTargetPos;
            _lastLOSPos = _losManager.IsTargetVisible(GlobalPosition)
                && _losManager.GetNearestLOSToTarget(PathfindingComponent?.GetSolidPoints()) is var (hasNearestLos, nearestLosPos)
                && hasNearestLos ? nearestLosPos : _initialPos;
        }

        return _lastLOSPos;
    }
}