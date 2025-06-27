using Godot;

public partial class Room : Node2D
{
    public override void _Ready()
    {
        base._Ready();
        Global.SetCurrentRoom(this);
    }

    public override void _ExitTree()
    {
        if (Global.GetCurrentRoom() == this)
            Global.SetCurrentRoom(null);
        base._ExitTree();
    }
}