using Godot;
using System.Collections.Generic;

public static class NodeExtensions
{
    public static T? GetChild<T>(this Node node) where T : class
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is T value)
            {
                return value;
            }
        }
        return null;
    }

    public static bool TryGetChild<T>(this Node node, out T? value) where T : class
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is T temp)
            {
                value = temp;
                return true;
            }
        }
        value = null;
        return false;
    }

    public static IEnumerable<T> GetChildren<T>(this Node node) where T : class
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is T value)
            {
                yield return value;
            }
        }
    }

    public static bool HasChild<T>(this Node node) where T : class
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is T)
            {
                return true;
            }
        }
        return false;
    }
}
