using Godot;
using System;

[Tool]
public partial class EnemyLayoutZigZag : HBoxContainer
{
    [Export] public int ZigZagMargin { get; set; } = 100;
    private const int MaxEnemyMembers = 6;
    private PackedScene? _enemyPanelScene;

    public MarginContainer? AddEnemyPanel()
    {
        if (GetChildCount() >= MaxEnemyMembers)
            return null;
        var panel = _enemyPanelScene?.Instantiate<MarginContainer>();
        AddChild(panel);
        UpdateLayout();
        return panel;
    }

    public void RemoveEnemyPanel(MarginContainer? panel)
    {
        if (panel?.GetParent() == this)
        {
            RemoveChild(panel);
            panel.QueueFree();
            UpdateLayout();
        }
    }

    public override void _Ready()
    {
        _enemyPanelScene = GD.Load<PackedScene>("res://src/huds/combat/panel/EnemyPanel.tscn");
        if (Engine.IsEditorHint())
        {
            ChildEnteredTree += OnChildAdded;
            ChildExitingTree += OnChildRemoved;
        }
        else
        {
            var childCount = GetChildCount();
            if (childCount <= MaxEnemyMembers) return;
            for (int i = childCount - 1; i >= MaxEnemyMembers; i--)
                GetChild(i).QueueFree();
        }
        UpdateLayout();
    }

    private void UpdateLayout()
    {
        if (!IsInstanceValid(this)) return;
        var childCount = GetChildCount();
        for (int i = 0; i < childCount; i++)
        {
            if (GetChild(i) is MarginContainer child)
            {
                child.AddThemeConstantOverride("margin_top", i % 2 == 0 ? ZigZagMargin : 0);
                child.AddThemeConstantOverride("margin_bottom", i % 2 == 1 ? ZigZagMargin : 0);
            }
        }
    }

    private void OnChildAdded(Node child)
        => CallDeferred(MethodName.UpdateLayout);
    private void OnChildRemoved(Node child)
        => CallDeferred(MethodName.UpdateLayout);
}