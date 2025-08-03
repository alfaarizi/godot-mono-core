// https://refactoring.guru/design-patterns/observer
using Godot;
using System;

public partial class EventBus : Node
{
    public static EventBus Instance { get; private set; } = null!;

    // Destination events
    [Signal] public delegate void DestinationAddedEventHandler(string name, Node2D destination);
    [Signal] public delegate void DestinationRemovedEventHandler(string name);

    // Character events
    [Signal] public delegate void CharacterAddedEventHandler(string name, Character character);
    [Signal] public delegate void CharacterRemovedEventHandler(string name);

    // Actionable events
    [Signal] public delegate void ActionableAddedEventHandler(string name, Actionable actionable);
    [Signal] public delegate void ActionableRemovedEventHandler(string name);

    // Camera events
    [Signal] public delegate void CameraChangedEventHandler(CameraComponent newCamera);

    // Room events
    [Signal] public delegate void RoomChangedEventHandler(Room newRoom);

    // Internal Data events
    public static event Action<string, NavigationData>? NavigationDataAdded;

    public static void EmitDestinationAdded(string name, Node2D destination)
        => Instance.EmitSignal(SignalName.DestinationAdded, name, destination);

    public static void EmitDestinationRemoved(string name)
        => Instance.EmitSignal(SignalName.DestinationRemoved, name);

    public static void EmitCharacterAdded(string name, Character character)
        => Instance.EmitSignal(SignalName.CharacterAdded, name, character);

    public static void EmitCharacterRemoved(string name)
        => Instance.EmitSignal(SignalName.CharacterRemoved, name);

    public static void EmitActionableAdded(string name, Actionable actionable)
        => Instance.EmitSignal(SignalName.ActionableAdded, name, actionable);

    public static void EmitActionableRemoved(string name)
        => Instance.EmitSignal(SignalName.ActionableRemoved, name);

    public static void EmitCameraChanged(CameraComponent newCamera)
        => Instance.EmitSignal(SignalName.CameraChanged, newCamera);

    public static void EmitRoomChanged(Room newRoom)
        => Instance.EmitSignal(SignalName.RoomChanged, newRoom);

    public static void EmitNavigationDataAdded(string roomName, NavigationData navigationData)
        => NavigationDataAdded?.Invoke(roomName, navigationData);

    public override void _Ready()
        => Instance = this;
}