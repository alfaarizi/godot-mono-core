using Godot;

[Tool]
public partial class PartyMemberPanel : MarginContainer
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
            if (isDisabled && _marginContainer != null)
                _marginContainer.Modulate = _disabledColor;
        }
    }

    private TextureButton? _textureButton;
    private MarginContainer? _marginContainer;
    private static readonly Color _normalColor = Color.Color8(255, 255, 255, 255);
    private static readonly Color _disabledColor = Color.Color8(100, 100, 100, 255);

    public override void _Ready()
    {
        _textureButton = GetNodeOrNull<TextureButton>("%TextureButton");
        _marginContainer = GetNodeOrNull<MarginContainer>("%MarginContainer");
        if (_textureButton != null)
        {
            _textureButton.MouseEntered += OnButtonHovered;
            _textureButton.MouseExited += OnButtonUnhovered;
        }
        OnButtonUnhovered();
    }

    private void OnButtonHovered()
    {
        if (!IsDisabled && _marginContainer != null)
            _marginContainer.Modulate = _normalColor;
    }
    private void OnButtonUnhovered()
    {
        if (!IsDisabled && _marginContainer != null)
            _marginContainer.Modulate = _disabledColor;
    }
}