using Godot;
using System;
using System.Collections.Generic;

public partial class LOSManager : Node2D
{
    [Export] public bool IsEnabled { get; set; } = true;
    [Export] public TileMapLayer? TileMapLayer { get; set; }
    [Export] public Node2D? Target { get; set; }
    [Export] public int TileRadius { get; set; } = 32;
    [Export] public float TileUpdateDistance { get; set; } = 96.0f;
    [Export] public int TileProcessedPerFrame { get; set; } = 60;
    [Export] public int NearestLOSUpdateFrames { get; set; } = 6;

    private readonly Dictionary<Vector2I, LOSTile> _tiles = new();
    private readonly Queue<Vector2I> _tilesUpdateQueue = new();
    private readonly List<Vector2I> _tilesRemoveList = new(300);
    private readonly PhysicsRayQueryParameters2D _rayQuery = new();

    private PhysicsDirectSpaceState2D? _space;
    private Vector2 _lastTargetPos = Vector2.Inf;
    private Vector2 _lastNearestLOS;
    private ulong _lastNearestLOSFrame;

    private float _tileSize;
    private int _tileRadiusSq;
    private float _tileRadiusInPixels;
    private float _tileRadiusInPixelsSq;

    public struct LOSTile
    {
        public Vector2I GridPos { get; set; }
        public Vector2 WorldPos { get; set; }
        public bool HasLOS { get; set; }
    }

    public Vector2I ToGridPos(Vector2 worldPosition)
        => TileMapLayer?.LocalToMap(TileMapLayer.ToLocal(worldPosition)) ?? Vector2I.Zero;

    public Vector2 ToWorldPos(Vector2I gridPosition)
        => TileMapLayer?.ToGlobal(TileMapLayer.MapToLocal(gridPosition)) ?? Vector2.Zero;

    public bool HasLOSAt(Vector2 globalPosition)
        => _tiles.TryGetValue(ToGridPos(globalPosition), out var tile) && tile.HasLOS;

    public IReadOnlyDictionary<Vector2I, LOSTile> GetTiles()
        => _tiles;

    public (bool, Vector2) GetNearestLOSToTarget(HashSet<Vector2I>? excludePoints = null)
    {
        if (Target == null) return (false, Vector2.Zero);

        var currentFrame = Engine.GetProcessFrames();
        if (currentFrame - _lastNearestLOSFrame >= (ulong)NearestLOSUpdateFrames)
        {
            var nearestDistSq = float.MaxValue;
            _lastNearestLOS = Vector2.Zero;

            foreach (var tile in _tiles.Values)
            {
                if (!tile.HasLOS || (excludePoints != null && excludePoints.Contains(tile.GridPos)))
                    continue;
                var distSq = tile.WorldPos.DistanceSquaredTo(Target.GlobalPosition);
                if (distSq < nearestDistSq)
                {
                    _lastNearestLOS = tile.WorldPos;
                    nearestDistSq = distSq;
                }
            }
            _lastNearestLOSFrame = currentFrame;
        }
        return (_lastNearestLOS != Vector2.Zero, _lastNearestLOS);
    }

    public bool IsTargetVisible(Vector2 globalPosition)
    {
        if (HasLOSAt(globalPosition)) return true;
        foreach (var tile in _tiles.Values)
            if (tile.HasLOS && HasLOS(globalPosition, tile.WorldPos)) return true;
        return false;
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
        {
            SetProcess(false);
            return;
        }
        _space = GetWorld2D().DirectSpaceState;
        _rayQuery.CollisionMask = 1;
        if (TileMapLayer?.TileSet != null)
        {
            _tileSize = TileMapLayer.TileSet.TileSize.X;
            _tileRadiusSq = TileRadius * TileRadius;
            _tileRadiusInPixels = TileRadius * _tileSize;
            _tileRadiusInPixelsSq = _tileRadiusInPixels * _tileRadiusInPixels;
        }
        Target ??= Global.GetCharacter("Player");
        if (Target == null) SetProcess(false);
    }

    public override void _Process(double delta)
    {
        if (!IsEnabled || Target == null || TileMapLayer == null) return;
        var pos = Target.GlobalPosition;
        if (_lastTargetPos.DistanceSquaredTo(pos) > TileUpdateDistance * TileUpdateDistance)
        {
            UpdateQueue(pos);
            _lastTargetPos = pos;
        }
        ProcessQueue();
    }

    private void UpdateQueue(Vector2 targetPos)
    {
        if (TileMapLayer == null) return;

        var centerPos = TileMapLayer.LocalToMap(TileMapLayer.ToLocal(targetPos));
        var centerGrid = new Vector2I(centerPos.X & ~1, centerPos.Y & ~1);

        _tilesRemoveList.Clear();
        foreach (var kvp in _tiles)
        {
            var distSq = kvp.Value.WorldPos.DistanceSquaredTo(targetPos);
            if (distSq > _tileRadiusInPixelsSq)
                _tilesRemoveList.Add(kvp.Key);
        }

        for (int i = 0; i < _tilesRemoveList.Count; i++)
            _ = _tiles.Remove(_tilesRemoveList[i]);

        _tilesUpdateQueue.Clear();
        var centerX = centerGrid.X;
        var centerY = centerGrid.Y;
        for (int x = -TileRadius; x <= TileRadius; x += 2)
        {
            var gridX = centerX + x;
            var xSq = x * x;
            for (int y = -TileRadius; y <= TileRadius; y += 2)
            {
                var gridY = centerY + y;
                var distSq = xSq + (y * y);
                if (distSq <= _tileRadiusSq)
                    _tilesUpdateQueue.Enqueue(new Vector2I(gridX, gridY));
            }
        }
    }

    private void ProcessQueue()
    {
        if (_tilesUpdateQueue.Count == 0 || Target == null || TileMapLayer == null) return;

        var targetPosition = Target.GlobalPosition;

        int processed = 0;
        while (_tilesUpdateQueue.Count > 0 && processed < TileProcessedPerFrame)
        {
            var gridPos = _tilesUpdateQueue.Dequeue();
            var worldPos = TileMapLayer.ToGlobal(TileMapLayer.MapToLocal(gridPos));
            var distSq = worldPos.DistanceSquaredTo(targetPosition);

            if (distSq > _tileRadiusInPixelsSq)
            {
                processed++;
                continue;
            }

            _tiles[gridPos] = new LOSTile
            {
                GridPos = gridPos,
                WorldPos = worldPos,
                HasLOS = HasLOS(targetPosition, worldPos)
            };
            processed++;
        }
    }

    private bool HasLOS(Vector2 from, Vector2 to)
    {
        if (_space == null) return false;
        _rayQuery.From = from; _rayQuery.To = to;
        return _space.IntersectRay(_rayQuery).Count == 0;
    }
}