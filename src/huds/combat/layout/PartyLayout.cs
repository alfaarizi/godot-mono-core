using Godot;
using System;

public partial class PartyLayout : HBoxContainer
{
    private const int MaxPartyMembers = 4;
    private PackedScene? _partyMemberPanelScene;

    public MarginContainer? AddPartyMember()
    {
        if (GetChildCount() >= MaxPartyMembers)
            return null;
        var panel = _partyMemberPanelScene?.Instantiate<MarginContainer>();
        AddChild(panel);
        return panel;
    }

    public void RemovePartyMember(Node? panel)
    {
        if (panel?.GetParent() == this)
        {
            RemoveChild(panel);
            panel.QueueFree();
        }
    }

    public override void _Ready()
    {
        _partyMemberPanelScene = GD.Load<PackedScene>("res://src/huds/combat/panel/PartyMemberPanel.tscn");
        var childCount = GetChildCount();
        if (childCount <= MaxPartyMembers) return;
        for (int i = childCount - 1; i >= MaxPartyMembers; i--)
            GetChild(i).QueueFree();
    }
}