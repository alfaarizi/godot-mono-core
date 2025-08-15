using Godot;

public partial class LOSDebugger : Node2D
{
    [Export]
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            _isEnabled = value;
            QueueRedraw();
        }
    }
    [Export] public LOSManager? LOSManager { get; private set; }

    private bool _isEnabled;
    private float _tileSize;
    private float _circleSize;
    private float _radius;
    private float _updateDistanceSq;

    private int _lastTileCount = -1;
    private Vector2 _lastTargetPos = Vector2.Inf;

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            SetProcess(false);
            return;
        }

        EventBus.Instance.RoomChanged += OnRoomChanged;

        var currentRoom = Global.GetCurrentRoom();
        if (currentRoom != null)
            OnRoomChanged(currentRoom);
    }

    public override void _ExitTree()
        => EventBus.Instance.RoomChanged -= OnRoomChanged;

    public override void _Process(double delta)
    {
        if (!IsEnabled || LOSManager?.Target == null) return;

        var tileCount = LOSManager.GetTiles().Count;
        var targetPos = LOSManager.Target.GlobalPosition;

        if (_lastTileCount != tileCount || _lastTargetPos.DistanceSquaredTo(targetPos) > _updateDistanceSq)
        {
            _lastTileCount = tileCount;
            _lastTargetPos = targetPos;
            QueueRedraw();
        }
    }

    public override void _Draw()
    {
        if (!IsEnabled || LOSManager == null || _tileSize == 0) return;

        var tiles = LOSManager.GetTiles();
        if (tiles.Count == 0) return;

        var targetPos = ToLocal(LOSManager.Target!.GlobalPosition);

        foreach (var tile in tiles.Values)
            DrawCircle(ToLocal(tile.WorldPos), _circleSize, tile.HasLOS ? Colors.Gold : Colors.Purple);
        DrawArc(targetPos, _radius, 0, Mathf.Tau, 32, Colors.Cyan, 1f);
    }

    private void OnRoomChanged(Room room)
    {
        LOSManager = room.GetNodeOrNull<LOSManager>("%LOSManager");
        if (LOSManager?.TileMapLayer?.TileSet != null)
        {
            _tileSize = LOSManager.TileMapLayer.TileSet.TileSize.X;
            _circleSize = _tileSize;
            _radius = LOSManager.TileRadius * _tileSize;
            _updateDistanceSq = LOSManager.TileUpdateDistance * LOSManager.TileUpdateDistance;
        }
    }
}