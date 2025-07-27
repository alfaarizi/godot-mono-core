using Godot;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public partial class Room : Node2D
{
    [Export] public TileMapLayer? TileMapRect { get; set; }
    private LOSManager? LOSManager;
#if DEBUG
    private Label? FPSLabel;
    private Button? LOSButton;
    private LOSDebugger? LOSDebugger;
#endif

    public override void _Ready()
    {
        base._Ready();
        Global.CurrentRoom = this;
        Global.CurrentCamera?.SetBoundsFromTileMap(TileMapRect);

        LOSManager = GetNodeOrNull<LOSManager>("%LOSManager");

#if DEBUG
        FPSLabel = GetNodeOrNull<Label>("%FPSLabel");
        LOSButton = GetNodeOrNull<Button>("%LOSButton");
        LOSDebugger = GetNodeOrNull<LOSDebugger>("%LOSDebugger");

        if (LOSButton != null)
        {
            LOSButton.Pressed += () =>
            {
                if (LOSDebugger == null) return;
                LOSDebugger.IsEnabled = !LOSDebugger.IsEnabled;
                LOSButton.Text = LOSDebugger.IsEnabled ? "LOS: On" : "LOS: Off";
            };
            LOSButton.Text = LOSDebugger?.IsEnabled == true ? "LOS: On" : "LOS: Off";
        }
#endif
    }


#if DEBUG
    public override void _Process(double delta)
    {
        if (FPSLabel == null) return;
        FPSLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
    }
#endif

    public LOSManager? GetLOSManager()
        => LOSManager;

    public override void _ExitTree()
    {
        if (Global.CurrentRoom == this)
            Global.CurrentRoom = null;
        base._ExitTree();
    }

}