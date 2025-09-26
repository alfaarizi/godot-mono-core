using Godot;

[Tool]
public partial class PartyMemberPanel : MarginContainer
{
    [Export] public bool IsSelected { get; set; }
    [Export] public bool IsDisabled { get; set; }

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
            _textureButton.FocusEntered += OnButtonStateChanged;
            _textureButton.FocusExited += OnButtonStateChanged;
            _textureButton.MouseEntered += () => _textureButton.GrabFocus();
            _textureButton.Pressed += () =>
            {
                GD.Print();
                IsSelected = !IsSelected;
                OnButtonStateChanged();
            };
        }
        OnButtonStateChanged();
    }

    private void OnButtonStateChanged()
    {
        if (_textureButton != null)
            _textureButton.Disabled = IsDisabled;
        if (_marginContainer != null)
        {
            bool isHighlighted = !IsDisabled && (IsSelected || _textureButton?.HasFocus() == true);
            _marginContainer.Modulate = isHighlighted ? _normalColor : _disabledColor;
        }
    }
}