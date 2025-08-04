using Godot;

public partial class Room : Node2D
{
    [Export] public TileMapLayer? TileMapRect { get; set; }
#if DEBUG
    private Label? RoomLabel;
    private Label? FPSLabel;
    private Button? LocaleButton;
    private Button? LOSButton;
    private LOSDebugger? LOSDebugger;
#endif

    public override void _Ready()
    {
        base._Ready();

#if DEBUG
        RoomLabel = GetNodeOrNull<Label>("%RoomLabel");
        FPSLabel = GetNodeOrNull<Label>("%FPSLabel");
        LocaleButton = GetNodeOrNull<Button>("%LocaleButton");
        LOSButton = GetNodeOrNull<Button>("%LOSButton");
        LOSDebugger = GetNodeOrNull<LOSDebugger>("%LOSDebugger");

        EventBus.EmitRoomChanged(this);

        if (RoomLabel != null)
            RoomLabel.Text = Tr("currentRoom").Replace("{room}", Name);

        if (LocaleButton != null)
        {
            var locale = TranslationServer.GetLocale();
            LocaleButton.Text = $"{Tr("locale")}: {Tr(locale[..2])}";
            LocaleButton.Modulate = locale.StartsWith("ja") ? Colors.Green : Colors.Red;
            LocaleButton.Pressed += () =>
            {
                var newLocale = TranslationServer.GetLocale().StartsWith("en") ? "ja" : "en_US";
                TranslationServer.SetLocale(newLocale);

                LocaleButton.Text = $"{Tr("locale")}: {Tr(newLocale[..2])}";
                LocaleButton.Modulate = newLocale.StartsWith("ja") ? Colors.Green : Colors.Red;

                if (RoomLabel != null)
                    RoomLabel.Text = Tr("currentRoom").Replace("{room}", Name);

                if (LOSDebugger != null && LOSButton != null)
                {
                    LOSButton.Text = $"LOS: {Tr(LOSDebugger.IsEnabled ? "on" : "off")}";
                    LOSButton.Modulate = LOSDebugger.IsEnabled ? Colors.Green : Colors.Red;
                }
            };
        }

        if (LOSDebugger != null && LOSButton != null)
        {
            var losStatus = LOSDebugger.IsEnabled ? "on" : "off";
            LOSButton.Text = $"LOS: {Tr(losStatus)}";
            LOSButton.Modulate = LOSDebugger.IsEnabled ? Colors.Green : Colors.Red;
            LOSButton.Pressed += () =>
            {
                LOSDebugger.IsEnabled = !LOSDebugger.IsEnabled;

                var newLostStatus = LOSDebugger.IsEnabled ? "on" : "off";
                LOSButton.Text = $"LOS: {Tr(newLostStatus)}";
                LOSButton.Modulate = LOSDebugger.IsEnabled ? Colors.Green : Colors.Red;
            };
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
}