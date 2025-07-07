using Godot;

[Tool]
public partial class Transfer : Prop
{
    [Export(PropertyHint.File, "*.tscn")] public string RoomPath { get; set; } = "";
    [Export] public string DestinationName { get; set; } = "";
    [Export] public bool RequiresInteraction { get; set; }

    private Area2D? _detectionArea;
    private Marker2D? _marker;
    private InputComponent? _inputComponent;
    private PromptComponent? _promptComponent;
    private SceneManager? _sceneManager;
    private Player? _playerEntered;
    private Vector2 _entryDirection;
    private bool _isTransported;

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint()) return;

        _detectionArea = GetNodeOrNull<Area2D>("%DetectionArea");
        _marker = GetNodeOrNull<Marker2D>("%Marker2D");
        _inputComponent = GetNodeOrNull<InputComponent>("%InputComponent");
        _promptComponent = GetNodeOrNull<PromptComponent>("%PromptComponent");
        _sceneManager = GetNode<SceneManager>("/root/SceneManager");

        Global.AddDestination(Name, this);

        if (_detectionArea != null)
        {
            _detectionArea.BodyEntered += OnPlayerEntered;
            _detectionArea.BodyExited += OnPlayerExited;
        }

        if (_marker != null)
        {
            Vector2 diff = GlobalPosition - _marker.GlobalPosition;
            _entryDirection = Mathf.Abs(diff.X) > Mathf.Abs(diff.Y)
                ? new Vector2(Mathf.Sign(diff.X), 0)
                : new Vector2(0, Mathf.Sign(diff.Y));
        }

        if (_inputComponent != null)
            _inputComponent.ActionPressed += OnActionPressed;
    }

    public override void _ExitTree()
    {
        Global.RemoveDestination(Name);

        if (_detectionArea != null)
        {
            _detectionArea.BodyEntered -= OnPlayerEntered;
            _detectionArea.BodyExited -= OnPlayerExited;
        }

        if (_inputComponent != null)
            _inputComponent.ActionPressed -= OnActionPressed;

        base._ExitTree();
    }

    private void OnPlayerEntered(Node body)
    {
        if (body is not Player player) return;

        _isTransported = false;
        _playerEntered = player;

        if (RequiresInteraction)
        {
            _promptComponent?.Show("Press Z to Enter");
            return;
        }

        if (player.MovementComponent != null)
            player.MovementComponent.LastDirectionChanged += OnPlayerLastDirectionChanged;

        // Check Initial Direction
        if (player.MovementComponent?.GetLastDirection().Dot(_entryDirection) > 0.0f)
        {
            _isTransported = true;
            Transport(player);
        }
    }

    private void OnPlayerLastDirectionChanged(Vector2 direction)
    {
        if (_playerEntered != null && !_isTransported && direction.Dot(_entryDirection) > 0.0f)
        {
            _isTransported = true;
            Transport(_playerEntered);
        }
    }

    private void OnPlayerExited(Node body)
    {
        if (body != _playerEntered) return;

        if (_playerEntered?.MovementComponent != null)
            _playerEntered.MovementComponent.LastDirectionChanged -= OnPlayerLastDirectionChanged;

        _isTransported = false;
        _playerEntered = null;

        if (RequiresInteraction)
            _promptComponent?.Hide();
    }

    private void OnActionPressed(StringName action)
    {
        if (_playerEntered == null || !RequiresInteraction || action != "ui_interact") return;
        Transport(_playerEntered);
    }

    private async void Transport(Player currentPlayer)
    {
        if (!string.IsNullOrEmpty(RoomPath))
        {
            _promptComponent?.Hide();
            currentPlayer.InputComponent?.SetEnabled(false);
            currentPlayer.MovementComponent?.SetEnabled(false);

            Room? currentRoom = Global.CurrentRoom;
            if (currentRoom == null || _sceneManager == null)
            {
                currentPlayer.InputComponent?.SetEnabled(true);
                currentPlayer.MovementComponent?.SetEnabled(true);
                return;
            }

            _sceneManager.ChangeScene(RoomPath, currentRoom.GetParent(), currentRoom);
            _ = await ToSignal(_sceneManager, SceneManager.SignalName.SceneLoadCompleted);

            if (Global.GetCharacter("Player") is Player newPlayer && Global.GetDestination(DestinationName) is Transfer newTransfer)
            {
                newPlayer.GlobalPosition = newTransfer._marker?.GlobalPosition ?? newTransfer.GlobalPosition;
                if (_entryDirection != Vector2.Zero)
                    newPlayer.MovementComponent?.SetLastDirection(_entryDirection);
            }
        }
        else if (!string.IsNullOrEmpty(DestinationName) && Global.GetDestination(DestinationName) is Transfer currentTransfer)
        {
            _promptComponent?.Hide();
            currentPlayer.GlobalPosition = currentTransfer._marker?.GlobalPosition ?? currentTransfer.GlobalPosition;
        }
    }
}