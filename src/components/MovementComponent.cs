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
    [Signal] public delegate void LastDirectionChangedEventHandler(Vector2 direction);
    [Signal] public delegate void MovementCompletedEventHandler();
    [Export] public PhysicsBody2D? Body { get; set; }
    [Export] public float MaxSpeed { get; set; } = 100.0f;
    [Export] public float Acceleration { get; set; } = 500.0f;
    [Export] public float Deceleration { get; set; } = 400.0f;

    private MovementMode _currentMovementMode = MovementMode.None;
    private Vector2 _velocity;
    private Vector2 _direction;
    private Vector2 _lastDirection = Vector2.Down;
    private Vector2 _targetPosition;
    private float _stoppingDistance = 1.0f;
    private bool _isMovingToTarget;

    #region Input Movement
    public void SetDirection(Vector2 direction)
    {
        _isMovingToTarget = false;
        _direction = direction.Normalized();
        if (_direction != Vector2.Zero && _direction != _lastDirection)
        {
            _lastDirection = _direction;
            _ = EmitSignal(SignalName.LastDirectionChanged, _lastDirection);
        }
        _currentMovementMode = direction.LengthSquared() > 0 ? MovementMode.Input : MovementMode.None;
    }
    #endregion

    #region Script Movement
    public void MoveToPosition(Vector2 targetPosition, float stoppingDistance = 1.0f)
    {
        if (IsAtTarget(targetPosition, stoppingDistance)) return;
        _isMovingToTarget = true;
        _targetPosition = targetPosition;
        _stoppingDistance = stoppingDistance;
        _currentMovementMode = MovementMode.Script;
    }

    public bool IsMovingToTarget()
        => _isMovingToTarget;
    #endregion

    #region Tweened Movement
    public void TweenToPosition(Vector2 targetPosition, float duration, float stoppingDistance = 1.0f)
    {
        if (IsAtTarget(targetPosition, stoppingDistance)) return;
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
    public bool IsAtTarget(Vector2 targetPosition, float stoppingDistance)
        => Body?.GlobalPosition.DistanceTo(targetPosition) <= stoppingDistance;

    public bool IsMoving()
        => _velocity.LengthSquared() > 0.1f || _isMovingToTarget;

    public bool IsColliding()
    {
        return Body switch
        {
            CharacterBody2D characterBody =>
                characterBody.IsOnWall() || characterBody.GetSlideCollisionCount() > 0,
            RigidBody2D rigidBody =>
                rigidBody.GetCollidingBodies().Count > 0, // requires more setup
            _ => false
        };
    }

    public void AddForce(Vector2 force)
        => _velocity += force;

    public MovementMode GetCurrentMovementMode()
        => _currentMovementMode;

    public Vector2 GetVelocity()
        => _velocity;

    public Vector2 GetDirection()
        => _direction;

    public Vector2 GetLastDirection()
        => _lastDirection;

    public void SetLastDirection(Vector2 lastDirection)
    {
        _lastDirection = lastDirection;
        _ = EmitSignal(SignalName.LastDirectionChanged, _lastDirection);
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
            Vector2 directionToTarget = _targetPosition - Body.GlobalPosition;
            if (directionToTarget.Length() <= _stoppingDistance)
            {
                _isMovingToTarget = false;
                _currentMovementMode = MovementMode.None;
                _ = EmitSignal(SignalName.MovementCompleted);
                _direction = Vector2.Zero;
            }
            else
            {
                _direction = directionToTarget.Normalized();
                if (_direction != Vector2.Zero && _direction != _lastDirection)
                {
                    _lastDirection = _direction;
                    _ = EmitSignal(SignalName.LastDirectionChanged, _lastDirection);
                }
            }
        }
        _velocity = CalculateVelocity(delta);
        MoveBody();
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