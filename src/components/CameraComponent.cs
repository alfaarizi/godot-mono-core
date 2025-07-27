using Godot;

public partial class CameraComponent : Component
{
    [Export] public Camera2D? Camera { get; set; }
    [Export] public Vector2 Offset { get; set; }
    [Export] public float Speed { get; set; } = 5.0f;

    private Rect2 _bounds = new(-10000000, -10000000, 20000000, 20000000);
    [Export]
    public Rect2 Bounds
    {
        get => _bounds;
        set
        {
            _bounds = value;
            if (Camera == null) return;
            Camera.LimitLeft = (int)value.Position.X;
            Camera.LimitTop = (int)value.Position.Y;
            Camera.LimitRight = (int)value.End.X;
            Camera.LimitBottom = (int)value.End.Y;
        }
    }

    private Node2D? _target;

    #region Script Movement
    public void MoveToPosition(Vector2 targetPosition)
    {
        _target = null;
        if (Camera != null)
            Camera.GlobalPosition = targetPosition + Offset;
        RefreshProcess();
    }
    #endregion

    #region Targeted Movement
    public void Follow(Node2D? target)
    {
        _target = target;
        RefreshProcess();
    }
    #endregion

    public void MakeCurrent()
    {
        if (Camera == null || !IsEnabled) return;
        Global.CurrentCamera = this;
        Camera.MakeCurrent();
        RefreshProcess();
        _ = CallDeferred(nameof(SetPosition));
    }

    public bool IsCurrent => Global.CurrentCamera == this;

    public void SetBoundsFromTileMap(TileMapLayer? tileMapLayer)
    {
        if (tileMapLayer == null) return;

        var usedRect = tileMapLayer.GetUsedRect();
        var quadrantSize = tileMapLayer.RenderingQuadrantSize;

        Bounds = new Rect2(
            tileMapLayer.ToGlobal(usedRect.Position * quadrantSize),
            tileMapLayer.ToGlobal(usedRect.End * quadrantSize)
        );
    }

    public bool IsInViewport(Vector2 position)
    {
        if (Camera == null) return false;
        var halfScreen = GetViewport().GetVisibleRect().Size / Camera.Zoom * 0.5f;
        return Mathf.Abs(position.X) <= halfScreen.X && Mathf.Abs(position.Y) <= halfScreen.Y;
    }

    public override void _Ready()
    {
        base._Ready();
        if (Engine.IsEditorHint())
        {
            SetPhysicsProcess(false);
            return;
        }

        if (Camera == null)
        {
            GD.PushError("Camera2D node not found in CameraComponent. Please assign a Camera2D node.");
            return;
        }

        _target = GetParent<Node2D>();

        Bounds = _bounds;
        EnabledChanged += OnEnabledChanged;
        RefreshProcess();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!IsEnabled || Camera == null || _target == null || !IsCurrent) return;
        Vector2 targetPosition = _target.GlobalPosition + Offset;
        Camera.GlobalPosition = Speed > 0
            ? Camera.GlobalPosition.Lerp(targetPosition, Speed * (float)delta)
            : targetPosition;
    }

    public override void _ExitTree()
    {
        EnabledChanged -= OnEnabledChanged;

        if (Global.CurrentCamera == this)
            Global.CurrentCamera = null;

        base._ExitTree();
    }

    private void SetPosition()
    {
        if (Camera == null || _target == null) return;
        Camera.GlobalPosition = _target.GlobalPosition + Offset;
    }

    private void RefreshProcess()
        => SetPhysicsProcess(IsEnabled && Camera != null && _target != null && IsCurrent);

    private void OnEnabledChanged(bool enabled)
        => RefreshProcess();

}