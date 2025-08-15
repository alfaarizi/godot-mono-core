using Godot;

[Tool]
public partial class DirectionMarker : Marker2D
{
    private Direction _direction;
    [Export]
    public Direction Direction
    {
        get => _direction;
        set
        {
            _direction = value;
            Rotation = value.ToRadians();
        }
    }
}