using Godot;

public partial class Room : Node2D
{
    [Export] public TileMapLayer? TileMapRect { get; set; }
#if DEBUG
    private Label? FPSLabel;
    private Button? LOSButton;
    private LOSDebugger? LOSDebugger;
#endif

    public override void _Ready()
    {
        base._Ready();

#if DEBUG
        FPSLabel = GetNodeOrNull<Label>("%FPSLabel");
        LOSButton = GetNodeOrNull<Button>("%LOSButton");
        LOSDebugger = GetNodeOrNull<LOSDebugger>("%LOSDebugger");

        EventBus.EmitRoomChanged(this);

        if (LOSButton == null) return;
        LOSButton.Pressed += () =>
        {
            if (LOSDebugger == null) return;
            LOSDebugger.IsEnabled = !LOSDebugger.IsEnabled;
            LOSButton.Text = LOSDebugger.IsEnabled ? "LOS: On" : "LOS: Off";
        };
        LOSButton.Text = LOSDebugger?.IsEnabled == true ? "LOS: On" : "LOS: Off";
#endif
    }

#if DEBUG
    public override void _Process(double delta)
    {
        if (FPSLabel == null) return;
        FPSLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
    }
#endif
}