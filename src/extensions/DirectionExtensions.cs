using Godot;
public static class DirectionExtensions
{
    private static readonly Vector2 DownLeft = new(-0.7071f, 0.7071f);
    private static readonly Vector2 UpLeft = new(-0.7071f, -0.7071f);
    private static readonly Vector2 UpRight = new(0.7071f, -0.7071f);
    private static readonly Vector2 DownRight = new(0.7071f, 0.7071f);

    private static readonly Vector2[] Vectors = {
        Vector2.Down,
        Vector2.Left,
        Vector2.Up,
        Vector2.Right,
        DownLeft,
        UpLeft,
        UpRight,
        DownRight
    };

    private static readonly float[] Radians = {
        0f,        // 0: 0
        1.5708f,   // 1: Math.PI/2
        3.1416f,   // 2: Math.PI
        -1.5708f,  // 3: -Math.PI/2
        0.7854f,   // 4: Math.PI/4
        2.3562f,   // 5: 3*Math.PI/4
        -2.3562f,  // 6: -3*Math.PI/4
        -0.7854f   // 7: -Math.PI/4
    };

    //  Direction Grid (index = x + y*3):
    //      0(none)  1(right)  2(left)  X
    //  0     [0]      [1]       [2]    <- Y none
    //  1     [3]      [4]       [5]    <- Y down
    //  2     [6]      [7]       [8]    <- Y up
    //  Y
    private static readonly Direction[] DirectionGrid = {
        Direction.Down, // fallback
        Direction.Right,
        Direction.Left,
        Direction.Down,
        Direction.DownRight,
        Direction.DownLeft,
        Direction.Up,
        Direction.UpRight,
        Direction.UpLeft
    };

    public static Vector2 ToVector(this Direction direction)
        => Vectors[(int)direction];

    public static float ToRadians(this Direction direction)
        => Radians[(int)direction];

    public static Direction ToDirection(this Vector2 vector)
    {
        if (vector == Vector2.Zero)
            return Direction.Down;

        // Map X: 0=none, 1=right, 2=left
        // Map Y: 0=none, 1=down, 2=up
        int x = vector.X > 0 ? 1 : (vector.X < 0 ? 2 : 0);
        int y = vector.Y > 0 ? 1 : (vector.Y < 0 ? 2 : 0);

        return DirectionGrid[x + (y * 3)];
    }
}