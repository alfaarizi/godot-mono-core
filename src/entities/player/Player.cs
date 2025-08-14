using Godot;

[Tool]
public partial class Player : Character
{
    public InputComponent? InputComponent { get; private set; }
    public MovementComponent? MovementComponent { get; private set; }
    public AnimationComponent? AnimationComponent { get; private set; }
    public CameraComponent? CameraComponent { get; private set; }
    private DirectionMarker? _directionMarker;

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint())
        {
            SetProcess(false);
            return;
        }

        InputComponent = GetNodeOrNull<InputComponent>("%InputComponent");
        if (InputComponent != null)
        {
            InputComponent.DirectionalInput += OnDirectionalInput;
        }

        MovementComponent = GetNodeOrNull<MovementComponent>("%MovementComponent");

        AnimationComponent = GetNodeOrNull<AnimationComponent>("%AnimationComponent");

        CameraComponent = GetNodeOrNull<CameraComponent>("%CameraComponent");

        CameraComponent?.MakeCurrent();

        _directionMarker = GetNodeOrNull<DirectionMarker>("%DirectionMarker");
    }

    public override void _Process(double delta)
    {
        if (AnimationComponent != null && MovementComponent != null)
        {
            bool isMoving = MovementComponent.IsMoving();
            Vector2 lastDirection = MovementComponent.GetLastDirection().ToVector();
            AnimationComponent.SetTreeParameter("conditions/is_moving", isMoving);
            AnimationComponent.SetTreeParameter("conditions/!is_moving", !isMoving);
            AnimationComponent.SetTreeParameter("Move/blend_position", lastDirection);
            AnimationComponent.SetTreeParameter("Idle/blend_position", lastDirection);
        }
    }

    public override void _ExitTree()
    {
        if (InputComponent != null)
            InputComponent.DirectionalInput -= OnDirectionalInput;
        base._ExitTree();
    }

    private void OnDirectionalInput(Vector2 inputVector)
    {
        MovementComponent?.SetDirection(inputVector);
        if (_directionMarker != null && inputVector != Vector2.Zero)
            _directionMarker.Direction = inputVector.ToDirection();
    }
}