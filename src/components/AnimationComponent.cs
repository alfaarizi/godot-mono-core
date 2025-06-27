using Godot;

public partial class AnimationComponent : Component
{
    [Signal] public delegate void AnimationFinishedEventHandler(StringName animationName);
    [Export] public AnimationPlayer? AnimationPlayer { get; set; }
    [Export] public AnimationTree? AnimationTree { get; set; }
    private StringName _currentAnimation = "";
    private bool _isUsingTree;

    #region AnimationTree Methods
    public void SetTreeParameter(StringName property, Variant value)
    {
        if (!IsEnabled || !_isUsingTree || AnimationTree == null) return;
        AnimationTree.Set($"parameters/{property}", value);
    }

    public Variant GetTreeParameter(StringName property)
    {
        return !_isUsingTree || AnimationTree == null
            ? default
            : AnimationTree.Get($"parameters/{property}");
    }

    public void TriggerOneshot(StringName property)
    {
        if (!IsEnabled || !_isUsingTree || AnimationTree == null) return;
        AnimationTree.Set($"parameters/{property}/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
    }
    #endregion

    #region AnimationPlayer Methods
    public void PlayAnimation(StringName animationName, float customBlend = -1.0f, float customSpeed = 1.0f)
    {
        if (!IsEnabled || AnimationPlayer == null) return;
        if (_isUsingTree)
        {
            GD.PrintErr("Cannot use PlayAnimation when AnimationTree is active. Use SetTreeParameter instead.");
            return;
        }
        _currentAnimation = animationName;
        AnimationPlayer.Play(animationName, customBlend, customSpeed);
    }

    public async void PlayAnimationAndWait(StringName animationName, float customSpeed = 1.0f)
    {
        if (!IsEnabled) return;
        PlayAnimation(animationName, -1, customSpeed);
        if (AnimationPlayer != null && AnimationPlayer.HasAnimation(animationName))
        {
            double duration = GetAnimationLength(animationName) / customSpeed;
            _ = await ToSignal(GetTree().CreateTimer(duration), SceneTreeTimer.SignalName.Timeout);
        }
    }

    public void StopAnimation(bool keepState = false)
    {
        if (!IsEnabled || AnimationPlayer == null) return;
        AnimationPlayer.Stop(keepState);
        _currentAnimation = "";
    }

    public void SeekAnimation(double seconds, bool update = false)
    {
        if (!IsEnabled || AnimationPlayer == null) return;
        AnimationPlayer.Seek(seconds, update);
    }

    public void SetAnimationPause(bool paused)
    {
        if (!IsEnabled || AnimationPlayer == null) return;
        AnimationPlayer.SetProcessMode(paused ? ProcessModeEnum.Disabled : ProcessModeEnum.Inherit);
    }

    public StringName GetCurrentAnimation()
    {
        return _isUsingTree || AnimationPlayer == null
            ? ""
            : _currentAnimation;
    }

    public double GetAnimationLength(StringName animationName)
    {
        return AnimationPlayer?.HasAnimation(animationName) != true
            ? 0.0
            : AnimationPlayer.GetAnimation(animationName).Length;
    }

    public bool IsAnimationPlaying(StringName animationName)
        => AnimationPlayer?.IsPlaying() == true && AnimationPlayer.CurrentAnimation == animationName;
    #endregion

    #region Shared Methods
    public void SetAnimationSpeed(float scale)
    {
        if (!IsEnabled) return;
        if (_isUsingTree) AnimationTree?.Set("parameters/TimeScale/scale", scale);
        else if (AnimationPlayer != null) AnimationPlayer.SpeedScale = scale;
    }

    public bool IsAnimationPlaying()
        => AnimationPlayer?.IsPlaying() == true;
    #endregion

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint()) return;
        _isUsingTree = AnimationTree?.Active == true;
        if (AnimationPlayer != null)
        {
            AnimationPlayer.Active = !_isUsingTree;
            AnimationPlayer.AnimationFinished += OnAnimationFinished;
        }
    }

    public override void _ExitTree()
    {
        if (AnimationPlayer != null) AnimationPlayer.AnimationFinished -= OnAnimationFinished;
        base._ExitTree();
    }

    private void OnAnimationFinished(StringName animationName)
    {
        _currentAnimation = "";
        _ = EmitSignal(SignalName.AnimationFinished, animationName);
    }
}