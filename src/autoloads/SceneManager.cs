using Godot;
using System;

public partial class SceneManager : Node
{
    [Signal] public delegate void SceneLoadCompletedEventHandler(Node loadedScene);

    private PackedScene? _loadingScreenScene;
    private LoadingScreen? _loadingScreen;
    private Timer? _loadingProgressTimer;
    private Action? _progressTimeoutHandler;
    private bool _isLoading;

    public void ChangeScene(string scenePath, Node? parentNode = null, Node? oldScene = null)
    {
        if (_isLoading)
        {
            GD.PushWarning("SceneManager: Load in progress, request ignored");
            return;
        }
        _isLoading = true;
        LoadScene(scenePath, parentNode ?? GetTree().Root, oldScene);
    }

    public override void _Ready()
    {
        _loadingScreenScene = GD.Load<PackedScene>("res://src/huds/LoadingScreen.tscn");
    }

    public override void _ExitTree()
    {
        if (_isLoading || _loadingProgressTimer != null || _loadingScreen != null)
            Cleanup();
        base._ExitTree();
    }

    private async void LoadScene(string scenePath, Node parentNode, Node? oldScene)
    {
        _loadingScreen = _loadingScreenScene?.Instantiate<LoadingScreen>();
        if (_loadingScreen == null)
        {
            GD.PrintErr("SceneManager: Failed to instantiate loading screen");
            _isLoading = false;
            return;
        }

        parentNode.AddChild(_loadingScreen);
        _loadingScreen.StartTransition("fade_to_black");

        // Wait for transition midpoint
        _ = await ToSignal(_loadingScreen, LoadingScreen.SignalName.TransitionMidpointReached);

        // Start threaded loading
        if (!ResourceLoader.Exists(scenePath) || ResourceLoader.LoadThreadedRequest(scenePath) != Error.Ok)
        {
            GD.PrintErr($"SceneManager: Failed to load scene {scenePath}");
            Cleanup();
            return;
        }

        // Monitor loading progress
        _loadingProgressTimer = new Timer { WaitTime = 0.1f };
        _progressTimeoutHandler = () => OnProgressTimeout(scenePath, parentNode, oldScene);

        _loadingProgressTimer.Timeout += _progressTimeoutHandler;
        parentNode.AddChild(_loadingProgressTimer);

        _loadingProgressTimer.Start();
    }

    private void OnProgressTimeout(string scenePath, Node parentNode, Node? oldScene)
    {
        var progressArray = new Godot.Collections.Array();
        var loadStatus = ResourceLoader.LoadThreadedGetStatus(scenePath, progressArray);
        switch (loadStatus)
        {
            case ResourceLoader.ThreadLoadStatus.InProgress:
                if (_loadingScreen != null && progressArray.Count > 0)
                    _loadingScreen.UpdateProgressBar((float)progressArray[0] * 100f);
                break;
            case ResourceLoader.ThreadLoadStatus.Loaded:
                CleanupTimer();
                var newScene = (ResourceLoader.LoadThreadedGet(scenePath) as PackedScene)?.Instantiate();
                if (newScene != null)
                {
                    parentNode.AddChild(newScene);
                    oldScene?.QueueFree();

                    _loadingScreen?.FinishTransition();
                    _loadingScreen = null;
                    _isLoading = false;

                    _ = EmitSignal(SignalName.SceneLoadCompleted, newScene);
                }
                else
                {
                    GD.PrintErr($"SceneManager: Failed to instantiate scene {scenePath}");
                    Cleanup();
                }
                break;
            case ResourceLoader.ThreadLoadStatus.InvalidResource:
            case ResourceLoader.ThreadLoadStatus.Failed:
                GD.PrintErr($"SceneManager: Scene loading failed {scenePath}");
                Cleanup();
                break;
            default:
                break;
        }
    }

    private void Cleanup()
    {
        CleanupTimer();
        _loadingScreen?.QueueFree();
        _loadingScreen = null;
        _isLoading = false;
    }

    private void CleanupTimer()
    {
        if (_loadingProgressTimer != null && _progressTimeoutHandler != null)
        {
            _loadingProgressTimer.Timeout -= _progressTimeoutHandler;
            _progressTimeoutHandler = null;
        }
        _loadingProgressTimer?.QueueFree();
        _loadingProgressTimer = null;
    }
}
