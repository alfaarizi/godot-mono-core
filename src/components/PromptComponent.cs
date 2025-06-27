using Godot;

public partial class PromptComponent : Component
{
    [Export] public string Text { get; set; } = string.Empty;
    [Export] public float SlideDuration { get; set; } = 0.3f;
    [Export] public float SlideDistance { get; set; } = 30.0f;

    private Tween? _tween;
    private Control? _control;
    private MarginContainer? _marginContainer;
    private Label? _label;
    private Vector2 _visiblePosition;
    private Vector2 _hiddenPosition;

    #region Timed Tween
    public void ShowAndHide(float displayDuration, string promptText = "")
    {
        if (!IsEnabled) return;
        SetText(promptText);
        _tween?.Kill();
        _tween = CreateTween();
        TweenShow(_tween);
        _ = _tween.TweenInterval(displayDuration);
        TweenHide(_tween);
    }
    #endregion

    #region Manual Tween
    public void Show(string promptText = "")
    {
        if (!IsEnabled) return;
        SetText(promptText);
        _tween?.Kill();
        _tween = CreateTween();
        TweenShow(_tween);
    }

    public void Hide()
    {
        if (!IsEnabled || !(_control?.Modulate.A > 0.1f)) return;
        _tween?.Kill();
        _tween = CreateTween();
        TweenHide(_tween);
    }
    #endregion

    #region Helper Methods
    private void SetText(string promptText)
    {
        if (_label != null)
            _label.Text = string.IsNullOrEmpty(promptText) ? Text : promptText;
    }

    private void TweenShow(Tween tween)
    {
        if (_control != null)
        {
            _ = tween.TweenProperty(_control, "modulate:a", 1.0f, SlideDuration)
                .SetTrans(Tween.TransitionType.Quart)
                .SetEase(Tween.EaseType.Out);
        }
        if (_marginContainer != null)
        {
            _marginContainer.Position = _hiddenPosition;
            _ = tween.Parallel().TweenProperty(_marginContainer, "position", _visiblePosition, SlideDuration)
                .SetTrans(Tween.TransitionType.Quart)
                .SetEase(Tween.EaseType.Out);
        }
    }

    private void TweenHide(Tween tween)
    {
        if (_control != null)
        {
            _ = tween.TweenProperty(_control, "modulate:a", 0.0f, SlideDuration)
                .SetTrans(Tween.TransitionType.Quart)
                .SetEase(Tween.EaseType.Out);
        }
        if (_marginContainer != null)
        {
            _ = tween.Parallel().TweenProperty(_marginContainer, "position", _hiddenPosition, SlideDuration)
                .SetTrans(Tween.TransitionType.Quart)
                .SetEase(Tween.EaseType.Out);
        }
    }
    #endregion

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint()) return;
        _control = GetNodeOrNull<Control>("%Control");
        _marginContainer = GetNodeOrNull<MarginContainer>("%MarginContainer");
        _label = GetNodeOrNull<Label>("%Label");
        if (_marginContainer != null)
        {
            _visiblePosition = _marginContainer.Position;
            _hiddenPosition = _visiblePosition + (Vector2.Down * SlideDistance);
        }
        if (_label != null)
            _label.Text = Text;
        // Start hidden
        _control?.SetModulate(new Color(1, 1, 1, 0));
    }

    public override void _ExitTree()
    {
        _tween?.Kill();
        base._ExitTree();
    }

#if DEBUG
    private bool _textVisible;
    public override void _UnhandledInput(InputEvent @event)
    {
        var currentScene = GetTree()?.CurrentScene;
        if (currentScene == null || currentScene.Name != Name) return;
        if (@event.IsActionPressed("ui_interact")) // Z
        {
            if (_textVisible) Hide();
            else Show("Debug: Manual Tween");
            _textVisible = !_textVisible;
        }
        else if (@event.IsActionPressed("ui_accept")) // Space
        {
            ShowAndHide(0.6f, "Debug: Timed Tween");
        }
    }
#endif
}