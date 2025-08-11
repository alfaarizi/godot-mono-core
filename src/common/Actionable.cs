using Godot;

public partial class Actionable : Area2D
{
    [Export] public bool RequiresInteraction { get; set; } = true;
    [Export] public Vector2 ActionDirection { get; set; }

    [Signal] public delegate void ActionRequestedEventHandler(Node2D actor);

    private InputComponent? _inputComponent;
    private PromptComponent? _promptComponent;
    private MovementComponent? _movementComponent;
    private Node2D? _actor;
    private bool _triggered;

    public void Action()
    {
        _triggered = true;
        _promptComponent?.Hide();
        if (_actor != null)
            _ = EmitSignal(SignalName.ActionRequested, _actor);
    }

    public override void _Ready()
    {
        _inputComponent = GetNodeOrNull<InputComponent>("%InputComponent");
        _promptComponent = GetNodeOrNull<PromptComponent>("%PromptComponent");

        BodyEntered += OnBodyEntered;
        BodyExited += OnBodyExited;
        if (_inputComponent != null)
            _inputComponent.ActionPressed += OnActionPressed;

        EventBus.EmitActionableAdded(Name, this);
    }

    public override void _ExitTree()
    {
        if (_inputComponent != null)
            _inputComponent.ActionPressed -= OnActionPressed;

        EventBus.EmitActionableRemoved(Name);

        base._ExitTree();
    }

    private void OnBodyEntered(Node body)
    {
        if (body is not Node2D actor || !actor.IsInGroup("action_actor")) return;

        _actor = actor;
        _movementComponent = actor.GetNodeOrNull<MovementComponent>("%MovementComponent");
        _triggered = false;

        if (RequiresInteraction)
            _promptComponent?.Show();
        else if (ActionDirection == Vector2.Zero)
            _ = EmitSignal(SignalName.ActionRequested, actor);
        else if (_movementComponent != null && IsValidDirection(_movementComponent.GetLastDirection()))
            Action();
        else if (_movementComponent != null)
            _movementComponent.LastDirectionChanged += OnLastDirectionChanged;
    }

    private void OnBodyExited(Node body)
    {
        if (body != _actor) return;

        if (_movementComponent != null)
            _movementComponent.LastDirectionChanged -= OnLastDirectionChanged;

        _promptComponent?.Hide();
        _actor = null;
        _movementComponent = null;
    }

    private void OnLastDirectionChanged(Vector2 dir)
    {
        if (!_triggered && IsValidDirection(dir))
            Action();
    }

    private void OnActionPressed(StringName action)
    {
        if (_actor == null || _movementComponent == null) return;
        if (RequiresInteraction && action == "ui_interact" && IsValidDirection(_movementComponent.GetLastDirection()))
            Action();
    }

    private bool IsValidDirection(Vector2 dir)
        => dir.Dot(ActionDirection) > 0;
}