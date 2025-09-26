using Godot;
using System.Collections.Generic;

public static class NodeExtensions
{
    public static T? GetChild<T>(this Node node, bool recursive = false) where T : class
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is T value) return value;
            if (recursive)
            {
                var result = child.GetChild<T>(true);
                if (result != null)
                    return result;
            }
        }
        return null;
    }

    public static bool TryGetChild<T>(this Node node, out T? value, bool recursive = false) where T : class
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is T temp)
            {
                value = temp;
                return true;
            }
            if (recursive && child.TryGetChild(out temp!, true))
            {
                value = temp;
                return true;
            }
        }
        value = null;
        return false;
    }

    public static IEnumerable<T> GetChildren<T>(this Node node, bool recursive = false) where T : class
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is T value) yield return value;
            if (recursive)
            {
                foreach (var descendant in child.GetChildren<T>(true))
                    yield return descendant;
            }
        }
    }

    public static void GetChildren<T>(this Node node, List<T> results, bool recursive = false) where T : class
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is T value)
                results.Add(value);
            if (recursive)
                child.GetChildren(results, true);
        }
    }

    public static bool HasChild<T>(this Node node, bool recursive = false) where T : class
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is T) return true;
            if (recursive && child.HasChild<T>(true))
                return true;
        }
        return false;
    }
}