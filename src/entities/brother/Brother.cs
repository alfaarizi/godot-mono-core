using Godot;

[Tool]
public partial class Brother : Character
{
    public MovementComponent? MovementComponent { get; private set; }
    public AnimationComponent? AnimationComponent { get; private set; }
    private Room? _room;

    private LOSManager? _losManager;
    private Vector2 _initialPosition;
    private Vector2 _targetPosition = Vector2.Zero;

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint())
        {
            SetProcess(false);
            return;
        }

        _ = CallDeferred(nameof(SetRoomAndPosition));

        MovementComponent = GetNodeOrNull<MovementComponent>("%MovementComponent");

        AnimationComponent = GetNodeOrNull<AnimationComponent>("%AnimationComponent");

        _losManager = GetNodeOrNull<LOSManager>("%LOSManager");

    }

    private void SetRoomAndPosition()
    {
        _room = Global.CurrentRoom;
        _initialPosition = GlobalPosition;
    }

    public override void _Draw()
    {
        if (Engine.IsEditorHint()) return;

        // Draw line from Brother to target position
        if (_targetPosition != Vector2.Zero)
        {
            Vector2 localTarget = ToLocal(_targetPosition);
            DrawLine(Vector2.Zero, localTarget, Colors.Red, 3.0f);
            DrawCircle(localTarget, 10.0f, Colors.Yellow);
        }

        Vector2 localInitial = ToLocal(_initialPosition);
        DrawLine(Vector2.Zero, localInitial, Colors.Blue, 2.0f);
        DrawCircle(localInitial, 8.0f, Colors.Cyan);
    }

    public override void _Process(double delta)
    {
        if (MovementComponent == null || AnimationComponent == null || _losManager == null || _losManager.Target == null) return;

        if (_losManager.IsTargetVisible(GlobalPosition))
        {
            var (hasLOS, targetPosition) = _losManager.GetNearestLOSToTarget(GlobalPosition);
            if (hasLOS)
            {
                _targetPosition = targetPosition;
                MovementComponent.MoveToPosition(targetPosition);
            }
        }
        else
        {
            _targetPosition = _initialPosition;
            MovementComponent.MoveToPosition(_initialPosition);
        }

        bool isMoving = MovementComponent.IsMoving();
        Vector2 lastDirection = MovementComponent.GetLastDirection();
        AnimationComponent.SetTreeParameter("conditions/is_moving", isMoving);
        AnimationComponent.SetTreeParameter("conditions/!is_moving", !isMoving);
        AnimationComponent.SetTreeParameter("Move/blend_position", lastDirection);
        AnimationComponent.SetTreeParameter("Idle/blend_position", lastDirection);

        QueueRedraw();
    }
}