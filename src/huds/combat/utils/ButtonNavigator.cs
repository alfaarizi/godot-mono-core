using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class ButtonNavigator : Node
{
    private readonly List<Control> _buttons = new();
    private int _index;

    public override void _Ready() => RefreshButtons();

    public override void _Input(InputEvent @event)
    {
        if (!@event.IsPressed()) return;

        if (@event.IsAction("ui_right")) Navigate(1);
        else if (@event.IsAction("ui_left")) Navigate(-1);
        else if (@event.IsAction("ui_accept")) Activate();
    }

    public void RefreshButtons()
    {
        _buttons.Clear();
        foreach (Control button in GetTree().GetNodesInGroup("nav_buttons").Cast<Control>())
        {
            if (button.Visible)
                _buttons.Add(button);
        }
        SetSelected(0);
    }

    private void Navigate(int direction)
    {
        if (_buttons.Count == 0) return;
        ClearHover(_buttons[_index]);
        _index = (_index + direction + _buttons.Count) % _buttons.Count;
        SetSelected(_index);
    }

    private void SetSelected(int index)
    {
        if (index < 0 || index >= _buttons.Count) return;
        SimulateHover(_buttons[index]);
        _buttons[index].GrabFocus();
        _index = index;
    }

    private static void SimulateHover(Control button)
    {
        if (button is BaseButton baseButton)
            _ = baseButton.EmitSignal(Control.SignalName.MouseEntered);
    }

    private static void ClearHover(Control button)
    {
        if (button is BaseButton baseButton)
            _ = baseButton.EmitSignal(Control.SignalName.MouseExited);
    }

    private void Activate()
    {
        if (_index < _buttons.Count && _buttons[_index] is BaseButton btn)
            _ = btn.EmitSignal(BaseButton.SignalName.Pressed);
    }
}