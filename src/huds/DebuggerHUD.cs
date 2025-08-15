using Godot;

public partial class DebuggerHUD : CanvasLayer
{
    private Room? _room;
    private Label? _roomLabel;
    private Label? _fpsLabel;
    private Button? _localeButton;
    private Button? _losButton;
    private LOSDebugger? _losDebugger;

    public override void _Ready()
    {
        _room = Global.GetCurrentRoom();
        _roomLabel = GetNodeOrNull<Label>("%RoomLabel");
        _fpsLabel = GetNodeOrNull<Label>("%FPSLabel");
        _localeButton = GetNodeOrNull<Button>("%LocaleButton");
        _losButton = GetNodeOrNull<Button>("%LOSButton");
        _losDebugger = GetNodeOrNull<LOSDebugger>("%LOSDebugger");

        EventBus.Instance.RoomChanged += OnRoomChanged;

        if (_localeButton != null)
        {
            _localeButton.Pressed += OnLocaleButtonPressed;
            _localeButton.FocusMode = Control.FocusModeEnum.None;
        }

        if (_losButton != null)
        {
            _losButton.Pressed += OnLOSButtonPressed;
            _losButton.FocusMode = Control.FocusModeEnum.None;
        }

        var currentRoom = Global.GetCurrentRoom();
        if (currentRoom != null)
            OnRoomChanged(currentRoom);
    }

    public override void _ExitTree()
        => EventBus.Instance.RoomChanged -= OnRoomChanged;

    public override void _Process(double delta)
    {
        if (_fpsLabel == null) return;
        _fpsLabel.Text = $"FPS: {Engine.GetFramesPerSecond()}";
    }

    private void OnRoomChanged(Room room)
    {
        _room = room;
        UpdateRoomLabel();
        UpdateLocaleButton();
        UpdateLOSButton();
    }

    private void OnLocaleButtonPressed()
    {
        var newLocale = TranslationServer.GetLocale().StartsWith("en") ? "ja" : "en_US";
        TranslationServer.SetLocale(newLocale);
        UpdateRoomLabel();
        UpdateLocaleButton();
        UpdateLOSButton();
    }

    private void OnLOSButtonPressed()
    {
        if (_losDebugger == null) return;
        _losDebugger.IsEnabled = !_losDebugger.IsEnabled;
        UpdateLOSButton();
    }

    private void UpdateRoomLabel()
    {
        if (_roomLabel == null || _room == null) return;
        _roomLabel.Text = Tr("currentRoom").Replace("{room}", _room.Name);
    }

    private void UpdateLocaleButton()
    {
        if (_localeButton == null) return;
        var locale = TranslationServer.GetLocale();
        _localeButton.Text = $"{Tr("locale")}: {Tr(locale[..2])}";
        _localeButton.Modulate = locale.StartsWith("ja") ? Colors.Green : Colors.Red;
    }

    private void UpdateLOSButton()
    {
        if (_losButton == null || _losDebugger == null) return;
        var losStatus = _losDebugger.IsEnabled ? "on" : "off";
        _losButton.Text = $"LOS: {Tr(losStatus)}";
        _losButton.Modulate = _losDebugger.IsEnabled ? Colors.Green : Colors.Red;
    }
}