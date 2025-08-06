using Godot;
using System.Collections.Generic;

public class NavigationData
{
    public AStarGrid2D Grid { get; set; }
    public HashSet<Vector2I> SolidPoints { get; set; }

    public NavigationData(AStarGrid2D grid, HashSet<Vector2I> solidPoints)
    {
        Grid = grid;
        SolidPoints = solidPoints;
    }
}