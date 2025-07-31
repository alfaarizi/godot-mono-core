using Godot;

[Tool]
public partial class Character : CharacterBody2D,
    IEntity,
    IDataProvider<CharacterData>
{
    [Export] public Sprite2D? Sprite { get; set; }
    [Export] public CollisionShape2D? CollisionShape { get; set; }
    [Export] public CharacterData? Data { get => _data; set => SetData(value); }
    private CharacterData? _data;

    public void SetData(CharacterData? data)
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
        EventBus.EmitCharacterAdded(Name, this);
    }

    public override void _ExitTree()
    {
        if (!Engine.IsEditorHint())
            EventBus.EmitCharacterRemoved(Name);
        base._ExitTree();
    }
}