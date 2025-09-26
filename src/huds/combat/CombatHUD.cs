using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CombatHUD : CanvasLayer
{
    private Control? _hostilesGroup;
    private Control? _alliesGroup;

    private readonly List<Node> _hostiles = new();
    private readonly List<Node> _allies = new();

    public override void _Ready()
    {
        _hostilesGroup = GetNodeOrNull<Control>("%HostilesGroup");
        _alliesGroup = GetNodeOrNull<Control>("%AlliesGroup");
        foreach (var hostile in GetTree().GetNodesInGroup("hostiles"))
            _hostiles.Add(hostile);
        foreach (var ally in GetTree().GetNodesInGroup("allies"))
            _allies.Add(ally);
    }
}