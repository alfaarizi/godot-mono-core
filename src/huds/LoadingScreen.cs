using Godot;

public partial class LoadingScreen : Node
{
    [Signal] public delegate void TransitionMidpointReachedEventHandler();

    [Export(PropertyHint.Enum, "Normal:1,Fast:2,Very Fast:3")] public int FadeSpeed { get; set; } = 1;

    private AnimationPlayer? _animationPlayer;
    private Label? _label;
    private ProgressBar? _progressBar;
    private Timer? _progressBarTimer;
    private StringName _startAnimationName = string.Empty;

    public void StartTransition(StringName animationName)
    {
        if (_animationPlayer != null)
        {
            _startAnimationName = _animationPlayer.HasAnimation(animationName)
                ? animationName
                : "fade_to_black";
            _animationPlayer.SpeedScale = FadeSpeed;
            _animationPlayer.Play(_startAnimationName);
        }
        _progressBarTimer?.Start();
    }

    public async void FinishTransition()
    {
        _progressBarTimer?.Stop();
        if (_animationPlayer != null)
        {
            StringName endAnimationName = _startAnimationName.ToString().Replace("to", "from");
            endAnimationName = _animationPlayer.HasAnimation(endAnimationName)
                ? endAnimationName
                : "fade_from_black";
            _animationPlayer.SpeedScale = FadeSpeed;
            _animationPlayer.Play(endAnimationName);
            _ = await ToSignal(_animationPlayer, AnimationMixer.SignalName.AnimationFinished);
        }
        QueueFree();
    }

    public void UpdateProgressBar(float progress)
    {
        if (_progressBar != null)
            _progressBar.Value = progress;
    }

    public void OnTransitionMidpoint()
        => EmitSignal(SignalName.TransitionMidpointReached);

    public override void _Ready()
    {
        _animationPlayer = GetNodeOrNull<AnimationPlayer>("%AnimationPlayer");
        _label = GetNodeOrNull<Label>("%Label");
        _progressBar = GetNodeOrNull<ProgressBar>("%ProgressBar");
        _progressBarTimer = GetNodeOrNull<Timer>("%Timer");
        if (_label != null && _progressBar != null && _progressBarTimer != null)
        {
            _label.Visible = false;
            _progressBar.Visible = false;
            _progressBarTimer.Timeout += OnProgressBarTimerTimeout;
        }
    }

    public override void _ExitTree()
    {
        if (_progressBarTimer != null)
            _progressBarTimer.Timeout -= OnProgressBarTimerTimeout;
        base._ExitTree();
    }

    private void OnProgressBarTimerTimeout()
    {
        if (_label != null && _progressBar != null)
        {
            _label.Visible = true;
            _progressBar.Visible = true;
        }
    }
}