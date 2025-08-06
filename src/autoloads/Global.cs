using Godot;
using System;
using System.Collections.Generic;

public partial class Global : Node
{
    private static readonly Dictionary<string, Node2D> _destinations = new();
    private static readonly Dictionary<string, Character> _characters = new();
    private static readonly Dictionary<string, Actionable> _actionables = new();
    private static CameraComponent? _currentCamera;
    private static Room? _currentRoom;
    private static readonly Dictionary<string, NavigationData> _navigationData = new();

    public static Node2D? GetDestination(string name)
        => _destinations.GetValueOrDefault(name);

    public static Character? GetCharacter(string name)
        => _characters.GetValueOrDefault(name);

    public static Actionable? GetActionable(string name)
        => _actionables.GetValueOrDefault(name);

    public static IEnumerable<Actionable> GetActionables()
        => _actionables.Values;

    public static CameraComponent? GetCurrentCamera()
        => _currentCamera;

    public static Room? GetCurrentRoom()
        => _currentRoom;

    public static NavigationData? GetNavigationData(string roomName)
        => _navigationData.GetValueOrDefault(roomName);

    public override void _Ready()
    {
        EventBus.Instance.DestinationAdded += OnDestinationAdded;
        EventBus.Instance.DestinationRemoved += OnDestinationRemoved;
        EventBus.Instance.CharacterAdded += OnCharacterAdded;
        EventBus.Instance.CharacterRemoved += OnCharacterRemoved;
        EventBus.Instance.ActionableAdded += OnActionableAdded;
        EventBus.Instance.ActionableRemoved += OnActionableRemoved;
        EventBus.Instance.CameraChanged += OnCameraChanged;
        EventBus.Instance.RoomChanged += OnRoomChanged;
        EventBus.NavigationDataAdded += OnNavigationDataAdded;

    }

    public override void _ExitTree()
    {
        EventBus.Instance.DestinationAdded -= OnDestinationAdded;
        EventBus.Instance.DestinationRemoved -= OnDestinationRemoved;
        EventBus.Instance.CharacterAdded -= OnCharacterAdded;
        EventBus.Instance.CharacterRemoved -= OnCharacterRemoved;
        EventBus.Instance.ActionableAdded -= OnActionableAdded;
        EventBus.Instance.ActionableRemoved -= OnActionableRemoved;
        EventBus.Instance.CameraChanged -= OnCameraChanged;
        EventBus.Instance.RoomChanged -= OnRoomChanged;
        EventBus.NavigationDataAdded -= OnNavigationDataAdded;
        base._ExitTree();
    }

    private static void OnDestinationAdded(string name, Node2D destination)
        => _destinations[name] = destination;

    private static void OnDestinationRemoved(string name)
        => _destinations.Remove(name);

    private static void OnCharacterAdded(string name, Character character)
        => _characters[name] = character;

    private static void OnCharacterRemoved(string name)
        => _characters.Remove(name);

    private static void OnActionableAdded(string name, Actionable actionable)
        => _actionables[name] = actionable;

    private static void OnActionableRemoved(string name)
        => _actionables.Remove(name);

    private static void OnCameraChanged(CameraComponent newCamera)
        => _currentCamera = newCamera;

    private static void OnRoomChanged(Room newRoom)
    {
        _currentRoom = newRoom;
        _currentCamera?.SetBoundsFromTileMap(newRoom.TileMapRect);
    }

    private static void OnNavigationDataAdded(string roomName, NavigationData navigationData)
        => _navigationData[roomName] = navigationData;
}