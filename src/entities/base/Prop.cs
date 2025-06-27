using Godot;

[Tool]
public partial class Prop : StaticBody2D,
    IEntity,
    IDataProvider<PropData>
{
    [Export] public Sprite2D? Sprite { get; set; }
    [Export] public CollisionShape2D? CollisionShape { get; set; }
    [Export] public PropData? Data { get => _data; set => SetData(value); }
    private PropData? _data;

    public void SetData(PropData? data)
    {
        _data = data;
        if (Sprite != null)
            Sprite.Texture = data?.Sprite;
        if (CollisionShape != null)
            CollisionShape.Shape = data?.CollisionShape;
    }

    public override void _Ready()
    {
        base._Ready();
        SetData(_data);
        if (Engine.IsEditorHint())
        {
            SetPhysicsProcess(false);
            return;
        }
    }
}
