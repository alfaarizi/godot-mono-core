using Godot;

[Tool]
public partial class EnemyPanel : MarginContainer
{
    private bool isDisabled;
    [Export]
    public bool IsDisabled
    {
        get => isDisabled;
        set
        {
            if (isDisabled == value) return;
            isDisabled = value;
            if (_textureButton != null)
                _textureButton.Disabled = isDisabled;
            if (isDisabled && _selectionRect != null)
                _selectionRect.Visible = false;
        }
    }

    private TextureButton? _textureButton;
    private TextureRect? _selectionRect;

    public override void _Ready()
    {
        _textureButton = GetNodeOrNull<TextureButton>("%TextureButton");
        _selectionRect = GetNodeOrNull<TextureRect>("%SelectionRect");
        if (_textureButton != null)
        {
            _textureButton.MouseEntered += OnButtonHovered;
            _textureButton.MouseExited += OnButtonUnhovered;
        }
        OnButtonUnhovered();
    }

    private void OnButtonHovered()
    {
        if (!IsDisabled && _selectionRect != null)
            _selectionRect.Visible = true;
    }

    private void OnButtonUnhovered()
    {
        if (!IsDisabled && _selectionRect != null)
            _selectionRect.Visible = false;
    }
}