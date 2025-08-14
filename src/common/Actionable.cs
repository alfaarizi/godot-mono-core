using Godot;

public partial class Actionable : Area2D
{
    [Export] public bool ForceInteraction { get; set; } = true;
    [Export] public bool ForceDirection { get; set; } = true;
    [Export] public Direction ActionDirection { get; set; }

    [Signal] public delegate void ActionRequestedEventHandler(Node2D actor);

    private InputComponent? _inputComponent;
    private PromptComponent? _promptComponent;
    private MovementComponent? _movementComponent;
    private Node2D? _actor;
    private bool _triggered;
    private bool _promptShown;

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

        bool validDirection = !ForceDirection || (_movementComponent?.GetLastDirection() is Direction dir && dir.Matches(ActionDirection));

        if (validDirection && !ForceInteraction)
        {
            Action();
        }
        else if (validDirection)
        {
            _promptComponent?.Show();
            _promptShown = true;
        }
        else
        {
            _promptComponent?.Hide();
            _promptShown = false;
        }

        if (_movementComponent != null && (ForceInteraction || !validDirection))
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

    private void OnLastDirectionChanged(int direction)
    {
        if (_triggered) return;

        bool validDirection = ((Direction)direction).Matches(ActionDirection);

        if (!validDirection)
        {
            _promptComponent?.Hide();
            _promptShown = false;
            return;
        }

        if (!ForceInteraction)
        {
            Action();
            return;
        }

        if (!_promptShown)
        {
            _promptComponent?.Show();
            _promptShown = true;
        }
    }

    private void OnActionPressed(StringName action)
    {
        if (_actor == null || _movementComponent == null) return;
        if (ForceInteraction && action == "ui_interact" && (!ForceDirection || _movementComponent.GetLastDirection().Matches(ActionDirection)))
            Action();
    }
}