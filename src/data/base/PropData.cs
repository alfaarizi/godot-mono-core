using Godot;

[GlobalClass, Tool]
public partial class PropData : Data
{
    [Export] public required string Name { get; set; }
    [Export] public required Texture2D Sprite { get; set; }
    [Export] public required Shape2D CollisionShape { get; set; }
}