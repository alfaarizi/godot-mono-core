using Godot;

public interface IEntity
{
    Sprite2D? Sprite { get; }
    CollisionShape2D? CollisionShape { get; }
}