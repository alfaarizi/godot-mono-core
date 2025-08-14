using Godot;

public partial class Actionable : Area2D
{
    [Signal] public delegate void ActionRequestedEventHandler(Node2D actor);
    [Signal] public delegate void ActionCompletedEventHandler();

    /// <summary>Destroys this object after first use</summary>
    [Export] public bool OneShot { get; set; } = false;
    /// <summary>Requires manual input to trigger (false = auto-trigger)</summary>
    [Export] public bool ForceInteraction { get; set; } = true;
    /// <summary>Actor must face specific direction to interact</summary>
    [Export] public bool ForceDirection { get; set; } = true;
    /// <summary>Required facing direction for interaction</summary>
    [Export] public Direction ActionDirection { get; set; }
    /// <summary>Text displayed when interaction is available</summary>
    [Export]
    public string ActionPrompt
    {
        get => _promptText;
        set
        {
            _promptText = value;
            if (_promptComponent != null)
                _promptComponent.Text = value;
        }
    }

    public static Actionable? ActiveActionable { get; private set; }

    private PromptComponent? _promptComponent;
    private MovementComponent? _movementComponent;
    private Node2D? _actor;
    private bool _isPromptVisible;
    private string _promptText = string.Empty;

    public void Action()
    {
        _promptComponent?.Hide();
        if (_actor != null)
            _ = EmitSignal(SignalName.ActionRequested, _actor);
    }

    public override void _Ready()
    {
        _promptComponent = GetNodeOrNull<PromptComponent>("%PromptComponent");

        if (_promptComponent != null && !string.IsNullOrEmpty(_promptText))
            _promptComponent.Text = _promptText;

        BodyEntered += OnBodyEntered;

        BodyExited += OnBodyExited;

        ActionCompleted += OnActionCompleted;

        EventBus.EmitActionableAdded(Name, this);
    }

    public override void _ExitTree()
    {
        if (ActiveActionable == this)
            ActiveActionable = null;
        EventBus.EmitActionableRemoved(Name);
        base._ExitTree();
    }

    private void OnBodyEntered(Node body)
    {
        if (body is not Node2D actor || !actor.IsInGroup("action_actor")) return;

        _actor = actor;
        _movementComponent = actor.GetNodeOrNull<MovementComponent>("%MovementComponent");

        bool isValidDirection = !ForceDirection || (_movementComponent?.GetLastDirection() is Direction dir && dir.Matches(ActionDirection));

        if (isValidDirection && !ForceInteraction)
        {
            Action();
        }
        else if (isValidDirection)
        {
            ActiveActionable = this;
            _promptComponent?.Show();
            _isPromptVisible = true;
        }
        else
        {
            _promptComponent?.Hide();
            _isPromptVisible = false;
        }

        if (_movementComponent != null && (ForceInteraction || !isValidDirection))
            _movementComponent.LastDirectionChanged += OnLastDirectionChanged;
    }

    private void OnBodyExited(Node body)
    {
        if (body != _actor) return;

        if (_movementComponent != null)
            _movementComponent.LastDirectionChanged -= OnLastDirectionChanged;

        if (ActiveActionable == this)
            ActiveActionable = null;

        _promptComponent?.Hide();
        _isPromptVisible = false;
        _actor = null;
        _movementComponent = null;
    }

    private void OnLastDirectionChanged(int direction)
    {
        bool isValidDirection = !ForceDirection || ((Direction)direction).Matches(ActionDirection);

        if (!isValidDirection)
        {
            if (ActiveActionable == this)
                ActiveActionable = null;
            _promptComponent?.Hide();
            _isPromptVisible = false;
            return;
        }

        if (!ForceInteraction)
        {
            Action();
            return;
        }

        if (!_isPromptVisible)
        {
            ActiveActionable = this;
            _promptComponent?.Show();
            _isPromptVisible = true;
        }
    }

    private void OnActionCompleted()
    {
        if (OneShot)
            QueueFree();
    }
}