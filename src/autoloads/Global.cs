using Godot;
using System.Collections.Generic;

public partial class Global : Node
{

    #region Destinations
    private static readonly Dictionary<string, Node2D> _destinations = new();
    public static Node2D? GetDestination(string name)
        => _destinations.GetValueOrDefault(name);
    internal static void AddDestination(string name, Node2D destination)
        => _destinations[name] = destination;
    internal static void RemoveDestination(string name)
        => _destinations.Remove(name);
    #endregion

    #region Characters
    private static readonly Dictionary<string, Character> _characters = new();
    public static Character? GetCharacter(string name)
        => _characters.GetValueOrDefault(name);
    internal static void AddCharacter(string name, Character character)
        => _characters[name] = character;
    internal static void RemoveCharacter(string name)
        => _characters.Remove(name);
    #endregion

    #region Current Room
    private static Room? _currentRoom;
    public static Room? GetCurrentRoom()
        => _currentRoom;
    internal static void SetCurrentRoom(Room? room)
        => _currentRoom = room;
    #endregion
}