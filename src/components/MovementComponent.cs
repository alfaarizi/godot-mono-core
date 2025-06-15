using Godot;
using System;

public enum MovementMode
{
    None,
    Input,
    Script,
    Tweened
}

public partial class MovementComponent : Component
{
    [Signal] public delegate void MovementCompletedEventHandler();
    [Export] public PhysicsBody2D? Body { get; set; }
    [Export] public float MaxSpeed { get; set; } = 100.0f;
    [Export] public float Acceleration { get; set; } = 500.0f;
    [Export] public float Deceleration { get; set; } = 400.0f;

    private MovementMode _currentMovementMode = MovementMode.None;
    private Vector2 _velocity;
    private Vector2 _direction;
    private Vector2 _targetPosition;
    private float _stoppingDistance = 1.0f;
    private bool _isMovingToTarget;

    #region Input Movement
    public void SetDirection(Vector2 direction)
    {
        _isMovingToTarget = false;
        _direction = direction.Normalized();
        _currentMovementMode = direction.LengthSquared() > 0 ? MovementMode.Input : MovementMode.None;
    }
    #endregion

    #region Script Movement
    public void MoveToPosition(Vector2 targetPosition, float stoppingDistance = 1.0f)
    {
        _isMovingToTarget = true;
        _targetPosition = targetPosition;
        _stoppingDistance = stoppingDistance;
        _currentMovementMode = MovementMode.Script;
    }

    public bool IsMovingToTarget()
    {
        return _isMovingToTarget;
    }
    #endregion

    #region Tweened Movement
    public void TweenToPosition(Vector2 targetPosition, float duration)
    {
        _isMovingToTarget = false;
        _direction = Vector2.Zero;
        _currentMovementMode = MovementMode.Tweened;
        var tween = CreateTween();
        _ = tween.TweenProperty(Body, "global_position", targetPosition, duration);
        _ = tween.TweenCallback(Callable.From(() =>
        {
            _currentMovementMode = MovementMode.None;
            _ = EmitSignal(SignalName.MovementCompleted);
        }));
    }
    #endregion

    #region Shared Methods
    public void AddForce(Vector2 force)
    {
        _velocity += force;
    }

    public MovementMode GetCurrentMovementMode()
    {
        return _currentMovementMode;
    }

    public Vector2 GetDirection()
    {
        return _direction;
    }

    public Vector2 GetVelocity()
    {
        return _velocity;
    }

    public bool IsMoving()
    {
        return _velocity.LengthSquared() > 0.1f || _isMovingToTarget;
    }
    #endregion

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint())
        {
            SetPhysicsProcess(false);
            return;
        }
        Body ??= GetParent<PhysicsBody2D>();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsEnabled || Body == null) return;
        if (_isMovingToTarget)
        {
            _direction = CalculateDirection();
        }
        _velocity = CalculateVelocity(delta);
        MoveBody();
    }

    private Vector2 CalculateDirection()
    {
        Vector2 directionToTarget = _targetPosition - (Body?.GlobalPosition ?? Vector2.Zero);
        if (directionToTarget.Length() <= _stoppingDistance)
        {
            _isMovingToTarget = false;
            _currentMovementMode = MovementMode.None;
            _ = EmitSignal(SignalName.MovementCompleted);
            return Vector2.Zero;
        }
        return directionToTarget.Normalized();
    }

    private Vector2 CalculateVelocity(double delta)
    {
        Vector2 targetVelocity = _direction * MaxSpeed;
        float rate = targetVelocity == Vector2.Zero ? Deceleration : Acceleration;
        float lerpFactor = 1.0f - MathF.Exp(-rate * (float)delta);
        return _velocity.Lerp(targetVelocity, lerpFactor);
    }

    private void MoveBody()
    {
        switch (Body)
        {
            case CharacterBody2D characterBody:
                characterBody.Velocity = _velocity;
                _ = characterBody.MoveAndSlide();
                _velocity = characterBody.Velocity;
                break;
            case RigidBody2D rigidBody:
                rigidBody.LinearVelocity = _velocity;
                break;
            case AnimatableBody2D animatableBody:
                animatableBody.GlobalPosition += _velocity * (float)GetPhysicsProcessDeltaTime();
                break;
            default:
                break;
        }
    }
}