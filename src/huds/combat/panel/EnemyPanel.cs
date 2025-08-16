using Godot;

[Tool]
public partial class EnemyPanel : MarginContainer
{
    private bool isSelected;
    [Export]
    public bool IsSelected
    {
        get => isSelected;
        set
        {
            isSelected = value;
            if (_selectionRect != null)
                _selectionRect.Visible = isSelected;
        }
    }

    private TextureRect? _selectionRect;
    public override void _Ready()
    {
        _selectionRect = GetNodeOrNull<TextureRect>("%SelectionRect");
    }
}
