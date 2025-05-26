using Godot;
using System;

public partial class Player : CharacterBody2D
{
    public const float Speed = 500.0f;

    public override void _PhysicsProcess(double delta)
    {
        Vector2 direction = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");
        Velocity = direction * Speed;
        MoveAndSlide();
    }

    public override void _Ready()
    {
        GD.Print("Player is ready");
    }

}
