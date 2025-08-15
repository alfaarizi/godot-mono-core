using Godot;
using System;
using System.Collections.Generic;
public static class DirectionExtensions
{
    private static readonly Vector2 DownLeft = new(-0.7071068f, 0.7071068f);
    private static readonly Vector2 UpLeft = new(-0.7071068f, -0.7071068f);
    private static readonly Vector2 UpRight = new(0.7071068f, -0.7071068f);
    private static readonly Vector2 DownRight = new(0.7071068f, 0.7071068f);

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

    // Direction match bit masks (index = target Direction)
    // Each bit indicates a Direction that counts as a valid match for the target
    // Bit positions (LSB -> MSB) follow the Direction enum order
    private static readonly byte[] DirectionBitMasks = {
        0b10010001, // Down matches     : Down, DownLeft, DownRight
        0b00110010, // Left matches     : Left, UpLeft, DownLeft
        0b01100100, // Up matches       : Up, UpLeft, UpRight
        0b11001000, // Right matches    : Right, UpRight, DownRight
        0b00010011, // DownLeft matches : Down, Left, DownLeft
        0b00100110, // UpLeft matches   : Left, Up, UpLeft
        0b01001100, // UpRight matches  : Up, Right, UpRight
        0b10001001  // DownRight matches: Down, Right, DownRight
    };

    /// <summary>
    /// Converts a Direction to its Vector2.
    /// </summary>
    public static Vector2 ToVector(this Direction direction)
        => Vectors[(int)direction];

    /// <summary>
    /// Converts a Direction to its radian value.
    /// </summary>
    public static float ToRadians(this Direction direction)
        => Radians[(int)direction];

    /// <summary>
    /// Converts a Vector2 to the nearest Direction. Looses precision.
    /// </summary>
    public static Direction ToDirection(this Vector2 vector)
    {
        int x = vector.X > 0 ? 1 : (vector.X < 0 ? 2 : 0); // Map X: 0=none, 1=right, 2=left
        int y = vector.Y > 0 ? 1 : (vector.Y < 0 ? 2 : 0); // Map Y: 0=none, 1=down, 2=up
        return DirectionGrid[x + (y * 3)];
    }

    /// <summary>
    /// Checks if this direction matches the given direction, including adjacent directions.
    /// </summary>
    public static bool Matches(this Direction direction, Direction otherDirection)
        => (DirectionBitMasks[(int)otherDirection] & (1 << (int)direction)) != 0;

    /// <summary>
    /// Checks if this direction matches the given vector direction. Looses precision.
    /// </summary>
    public static bool Matches(this Direction direction, Vector2 otherDirection)
        => direction.Matches(otherDirection.ToDirection());

    /// <summary>
    /// Checks if this vector direction matches the given direction. Looses precision.
    /// </summary>
    public static bool Matches(this Vector2 direction, Direction otherDirection)
        => direction.ToDirection().Matches(otherDirection);
}