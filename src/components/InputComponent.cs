using Godot;
using System;
using System.Collections.Generic;

public partial class InputComponent : Component
{
    [Signal] public delegate void DirectionalInputEventHandler(Vector2 inputVector);
    [Signal] public delegate void ActionPressedEventHandler(StringName action);
    [Signal] public delegate void HoldStartedEventHandler(StringName action);
    [Signal] public delegate void HoldingEventHandler(StringName action, float duration);
    [Signal] public delegate void HoldEndedEventHandler(StringName action);

    [ExportGroup("Directional Actions")]
    [Export] public StringName LeftAction { get; set; } = "";
    [Export] public StringName RightAction { get; set; } = "";
    [Export] public StringName UpAction { get; set; } = "";
    [Export] public StringName DownAction { get; set; } = "";

    [ExportGroup("Input Actions")]
    [Export] public StringName[] PressActions { get; set; } = Array.Empty<StringName>();
    [Export] public StringName[] HoldActions { get; set; } = Array.Empty<StringName>();

    private Vector2 _currentInputVector;
    private bool _hasDirectionalActions;
    private readonly Dictionary<StringName, float> _holdDurations = new();

    #region Input Methods
    public Vector2 GetInputVector()
        => _currentInputVector;

    public float GetHoldDuration(StringName action)
        => _holdDurations.GetValueOrDefault(action);

    public bool IsHolding(StringName action)
        => _holdDurations.ContainsKey(action);
    #endregion

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint())
        {
            SetProcess(false);
            SetProcessUnhandledInput(false);
            return;
        }
        _hasDirectionalActions = LeftAction != "" && RightAction != "" && UpAction != "" && DownAction != "";
    }

    public override void _Process(double delta)
    {
        if (!IsEnabled) return;
        if (_hasDirectionalActions)
        {
            HandleDirectionalInput();
        }
        if (_holdDurations.Count > 0)
        {
            float deltaTime = (float)delta;
            foreach (var kvp in _holdDurations)
            {
                float newDuration = kvp.Value + deltaTime;
                _holdDurations[kvp.Key] = newDuration;
                _ = EmitSignal(SignalName.Holding, kvp.Key, newDuration);
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!IsEnabled) return;
        HandleKeyInputs(@event);
    }

    private void HandleDirectionalInput()
    {
        Vector2 newInputVector = Input.GetVector(LeftAction, RightAction, UpAction, DownAction);
        if (_currentInputVector != newInputVector)
        {
            _currentInputVector = newInputVector;
            _ = EmitSignal(SignalName.DirectionalInput, _currentInputVector);
        }
    }

    private void HandleKeyInputs(InputEvent @event)
    {
        foreach (var pressAction in PressActions)
        {
            if (@event.IsActionPressed(pressAction))
                _ = EmitSignal(SignalName.ActionPressed, pressAction);
        }
        foreach (var holdAction in HoldActions)
        {
            if (@event.IsActionPressed(holdAction))
            {
                _holdDurations[holdAction] = 0.0f;
                _ = EmitSignal(SignalName.HoldStarted, holdAction);
            }
            else if (@event.IsActionReleased(holdAction))
            {
                _ = _holdDurations.Remove(holdAction);
                _ = EmitSignal(SignalName.HoldEnded, holdAction);
            }
        }
    }
}