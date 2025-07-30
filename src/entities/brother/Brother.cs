using Godot;

[Tool]
public partial class Brother : Character
{
    public MovementComponent? MovementComponent { get; private set; }
    public AnimationComponent? AnimationComponent { get; private set; }
    public PathfindingComponent? PathfindingComponent { get; private set; }

    private LOSManager? _losManager;
    private Vector2 _initialPosition;
    private Vector2 _lastTarget;

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

        Vector2 target = GetTargetPosition();

        if (target != _lastTarget && !PathfindingComponent.IsPathfinding())
        {
            PathfindingComponent.SetPath(target);
            _lastTarget = target;
        }

        if (AnimationComponent != null)
        {
            bool moving = MovementComponent.IsMoving();
            Vector2 direction = MovementComponent.GetLastDirection();
            AnimationComponent.SetTreeParameter("conditions/is_moving", moving);
            AnimationComponent.SetTreeParameter("conditions/!is_moving", !moving);
            AnimationComponent.SetTreeParameter("Move/blend_position", direction);
            AnimationComponent.SetTreeParameter("Idle/blend_position", direction);
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