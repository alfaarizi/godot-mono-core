using Godot;

[GlobalClass]
public partial class Component : Node,
    IComponent
{
    [Signal] public delegate void EnabledChangedEventHandler(bool isEnabled);
    [Export] public bool IsEnabled { get => _isEnabled; set => SetEnabled(value); }
    private bool _isEnabled = true;

    public void SetEnabled(bool isEnabled)
    {
        if (_isEnabled == isEnabled) return;
        _isEnabled = isEnabled;
        _ = EmitSignal(SignalName.EnabledChanged, isEnabled);
    }
}