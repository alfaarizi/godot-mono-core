using Godot;

public partial class Room : Node2D
{
    [Export] public TileMapLayer? TileMapRect { get; set; }
    public override void _Ready()
    {
        base._Ready();
        Global.CurrentRoom = this;
        Global.CurrentCamera?.SetBoundsFromTileMap(TileMapRect);
    }

    public override void _ExitTree()
    {
        if (Global.CurrentRoom == this)
            Global.CurrentRoom = null;
        base._ExitTree();
    }
}