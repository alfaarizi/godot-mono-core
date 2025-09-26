using Godot;

public partial class Room : Node2D
{
    [Export] public TileMapLayer? TileMapRect { get; set; }

    public override void _Ready()
        => EventBus.EmitRoomChanged(this);
}