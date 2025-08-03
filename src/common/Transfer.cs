using Godot;

[Tool]
public partial class Transfer : Prop
{
    [Export(PropertyHint.File, "*.tscn")] public string RoomPath { get; set; } = "";
    [Export] public string DestinationName { get; set; } = "";
    [Export]
    public bool RequiresInteraction
    {
        get => _requiresInteraction;
        set
        {
            _requiresInteraction = value;
            if (_actionable != null)
                _actionable.RequiresInteraction = value;
        }
    }

    private Actionable? _actionable;
    private Marker2D? _marker;
    private SceneManager? _sceneManager;
    private bool _requiresInteraction = true;

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint()) return;

        _actionable = GetNodeOrNull<Actionable>("%Actionable");
        _marker = GetNodeOrNull<Marker2D>("%Marker2D");
        _sceneManager = GetNode<SceneManager>("/root/SceneManager");

        EventBus.EmitDestinationAdded(Name, this);

        if (_actionable != null)
        {
            _actionable.RequiresInteraction = _requiresInteraction;
            _actionable.ActionRequested += OnActionRequested;

            if (_marker == null) return;
            Vector2 diff = GlobalPosition - _marker.GlobalPosition;
            _actionable.ActionDirection = Mathf.Abs(diff.X) > Mathf.Abs(diff.Y)
                ? new Vector2(Mathf.Sign(diff.X), 0)
                : new Vector2(0, Mathf.Sign(diff.Y));
        }
    }

    public override void _ExitTree()
    {
        if (Engine.IsEditorHint()) return;

        EventBus.EmitDestinationRemoved(Name);

        if (_actionable != null)
            _actionable.ActionRequested -= OnActionRequested;

        base._ExitTree();
    }

    private void OnActionRequested(Node2D actor)
        => Transport(actor);

    private async void Transport(Node2D actor)
    {
        var inputComponent = actor.GetNodeOrNull<InputComponent>("%InputComponent");
        var movementComponent = actor.GetNodeOrNull<MovementComponent>("%MovementComponent");

        inputComponent?.SetEnabled(false);
        movementComponent?.SetEnabled(false);

        if (!string.IsNullOrEmpty(RoomPath))
        {
            // Transport to a destination in a new room
            Room? currentRoom = Global.GetCurrentRoom();
            if (currentRoom == null || _sceneManager == null)
                return;

            _sceneManager.ChangeScene(RoomPath, currentRoom.GetParent(), currentRoom);
            _ = await ToSignal(_sceneManager, SceneManager.SignalName.SceneLoadCompleted);

            var newActor = Global.GetCharacter(actor.Name);
            if (newActor == null) return;

            TransportToDestination(newActor);

            if (_actionable != null && _actionable.ActionDirection != Vector2.Zero)
            {
                var newMovementComponent = newActor.GetNodeOrNull<MovementComponent>("%MovementComponent");
                newMovementComponent?.SetLastDirection(_actionable.ActionDirection);
            }
        }
        else if (!string.IsNullOrEmpty(DestinationName))
        {
            // Transport to a destination in the same room
            TransportToDestination(actor);
        }

        inputComponent?.SetEnabled(true);
        movementComponent?.SetEnabled(true);
    }

    private void TransportToDestination(Node2D actor)
    {
        if (Global.GetDestination(DestinationName) is Transfer destination)
            actor.GlobalPosition = destination._marker?.GlobalPosition ?? destination.GlobalPosition;
    }

}