using Godot;
using System;

[Tool]
public partial class Player : Character
{
    private InputComponent? _inputComponent;
    private MovementComponent? _movementComponent;
    private AnimationComponent? _animationComponent;

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint())
        {
            SetProcess(false);
            return;
        }
        _inputComponent = GetNodeOrNull<InputComponent>("InputComponent");
        _movementComponent = GetNodeOrNull<MovementComponent>("MovementComponent");
        _animationComponent = GetNodeOrNull<AnimationComponent>("AnimationComponent");
        if (_inputComponent != null)
            _inputComponent.DirectionalInput += OnDirectionalInput;
    }

    public override void _Process(double delta)
    {
        if (_animationComponent != null && _movementComponent != null)
        {
            bool isMoving = _movementComponent.IsMoving();
            Vector2 direction = _movementComponent.GetDirection();
            _animationComponent.SetTreeParameter("conditions/is_moving", isMoving);
            _animationComponent.SetTreeParameter("conditions/!is_moving", !isMoving);
            _animationComponent.SetTreeParameter("Move/blend_position", direction);
            _animationComponent.SetTreeParameter("Idle/blend_position", direction);
        }
    }

    public override void _ExitTree()
    {
        if (_inputComponent != null)
            _inputComponent.DirectionalInput -= OnDirectionalInput;
        base._ExitTree();
    }

    private void OnDirectionalInput(Vector2 inputVector)
    {
        _movementComponent?.SetDirection(inputVector);
    }
}