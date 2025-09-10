using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public sealed class TestMap : MonoBehaviour
{
    [Flags]
    public enum CellFlags { None = 0, Wall = 1 << 0, Destructible = 1 << 1, Tower = 1 << 2, Object = 1 << 3 }

    private const string EVT_STAGE_LOADED = "EVT_STAGE_LOADED";

    public int Width { get; private set; }
    public int Height { get; private set; }
    public float CellSize { get; private set; }

    public CellFlags[,] cells { get; private set; }

    public event Action<int, int> OnCellChanged;

    public Vector2Int SpawnCellRC { get; private set; } = new Vector2Int(-1, -1);
    public Vector2Int BossSpawnCellRC { get; private set; } = new Vector2Int(-1, -1);
    public bool HasSpawnCell { get { return SpawnCellRC.x >= 0; } }
    public bool HasBossSpawnCell { get { return BossSpawnCellRC.x >= 0; } }

    public bool IsInitialized { get; private set; } = false;
    public event Action OnInitialized;

    private MapManager _map;
    private Vector3Int _origin;
    private Vector3Int _sizeCells;

    private void Awake()
    {
        // 기존 즉시 초기화 시도 (준비 안 됐으면 이벤트로 후속 초기화)
        _map = MapManager.Instance;
        if (_map == null || !_map.IsReady) { return; }

        InitializeFromMapIfReady();
    }

    private void OnEnable()
    {
        EventManager.Instance.Subscribe<string>(GameManager.EVT_STAGE_LOADED, OnStageLoaded);
    }

    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe(GameManager.EVT_STAGE_LOADED, (System.Action<string>)OnStageLoaded);
    }

    private void OnDestroy()
    {
        if (_map != null) { _map.OnCellChanged -= HandleMapCellChanged; }
    }

    private void OnStageLoaded(string stageId)
    {
        InitializeFromMapIfReady();
    }

    private void InitializeFromMapIfReady()
    {
        if (MapManager.Instance == null || !MapManager.Instance.IsReady) { return; }
        if (IsInitialized) { return; }

        _map = MapManager.Instance;

        _map.GetNavFrame(out _origin, out _sizeCells, out Vector3 cellSize);
        Width = _sizeCells.x;
        Height = _sizeCells.y;
        CellSize = cellSize.x;

        cells = new CellFlags[Height, Width];
        BuildSnapshotAll();

        Vector3Int spawnAbs;
        if (_map.TryGetSpawnCell(out spawnAbs))
        {
            Vector2Int rc = AbsCellToRC(spawnAbs);
            if (InBounds(rc.x, rc.y)) { SpawnCellRC = rc; }
        }

        Vector3Int bossAbs;
        if (_map.TryGetBossSpawnCell(out bossAbs))
        {
            Vector2Int rc = AbsCellToRC(bossAbs);
            if (InBounds(rc.x, rc.y)) { BossSpawnCellRC = rc; }
        }

        _map.OnCellChanged -= HandleMapCellChanged;
        _map.OnCellChanged += HandleMapCellChanged;

        IsInitialized = true;
        if (OnInitialized != null) { OnInitialized(); }
    }

    public Vector2Int WorldToCell(Vector3 world)
    {
        Vector3Int abs = _map.WorldToCell(world);
        Vector2Int rc = AbsCellToRC(abs);
        rc.x = Mathf.Clamp(rc.x, 0, Height - 1);
        rc.y = Mathf.Clamp(rc.y, 0, Width - 1);
        return rc;
    }

    public Vector3 CellToWorld(int r, int c)
    {
        Vector3Int abs = RCToAbsCell(r, c);
        return _map.CellCenterWorld(abs);
    }

    public bool InBounds(int r, int c) { return (r >= 0 && c >= 0 && r < Height && c < Width); }

    public bool IsWalkable(int r, int c)
    {
        if (!InBounds(r, c)) { return false; }

        if (_map != null && _map.HasPlayerBase)
        {
            Vector3Int abs = RCToAbsCell(r, c);
            if (abs == _map.PlayerBaseCell) { return true; }
        }

        CellFlags f = cells[r, c];
        return (f & (CellFlags.Wall | CellFlags.Destructible | CellFlags.Tower)) == 0;
    }

    public bool IsBuildable(int r, int c)
    {
        if (!InBounds(r, c)) { return false; }

        Vector3Int abs = RCToAbsCell(r, c);

        bool buildable, unbuildable, wall, destructible, deco, occupied, objects;
        _map.GetCellFlags(abs, out buildable, out unbuildable, out wall, out destructible, out deco, out occupied, out objects);

        if (!buildable) { return false; }
        if (wall || destructible || objects) { return false; }
        if (occupied) { return false; }

        GameObject _;
        if (MapManager.Instance != null && MapManager.Instance.TryGetTowerAt(abs, out _)) { return false; }

        return true;
    }

    public bool IsWall(int r, int c) { return InBounds(r, c) && (cells[r, c] & CellFlags.Wall) != 0; }
    public bool IsDestructible(int r, int c) { return InBounds(r, c) && (cells[r, c] & CellFlags.Destructible) != 0; }
    public bool HasTower(int r, int c) { return InBounds(r, c) && (cells[r, c] & CellFlags.Tower) != 0; }

    public bool TryGetSpawnCell(out Vector2Int rc) { rc = SpawnCellRC; return HasSpawnCell; }
    public bool TryGetBossSpawnCell(out Vector2Int rc) { rc = BossSpawnCellRC; return HasBossSpawnCell; }

    public void SetDestructible(int r, int c, bool on)
    {
        if (!InBounds(r, c)) { return; }
        Vector3Int abs = RCToAbsCell(r, c);
        if (!on) { _map.ConvertDestructibleToBuildable(abs); }
    }

    public void SetTower(int r, int c, bool on)
    {
        if (!InBounds(r, c)) { return; }
        Vector3Int abs = RCToAbsCell(r, c);
        if (on) { _map.MarkOccupied(abs); }
        else { _map.UnregisterTower(abs); }
    }

    private void BuildSnapshotAll()
    {
        for (int r = 0; r < Height; r++)
        {
            for (int c = 0; c < Width; c++)
            {
                cells[r, c] = SampleFlags(RCToAbsCell(r, c));
            }
        }
    }

    private void HandleMapCellChanged(Vector3Int absCell)
    {
        Vector2Int rc = AbsCellToRC(absCell);
        if (!InBounds(rc.x, rc.y)) { return; }

        cells[rc.x, rc.y] = SampleFlags(absCell);
        if (OnCellChanged != null) { OnCellChanged(rc.x, rc.y); }
    }

    private CellFlags SampleFlags(Vector3Int absCell)
    {
        if (_map != null && _map.HasPlayerBase && absCell == _map.PlayerBaseCell)
        {
            return CellFlags.None;
        }

        bool buildable, unbuildable, wall, destructible, deco, occupied, objects;
        _map.GetCellFlags(absCell, out buildable, out unbuildable, out wall, out destructible, out deco, out occupied, out objects);

        CellFlags f = CellFlags.None;
        if (wall) { f |= CellFlags.Wall; }
        if (destructible) { f |= CellFlags.Destructible; }
        if (objects) { f |= CellFlags.Object; }

        GameObject _;
        bool hasTowerGO = MapManager.Instance.TryGetTowerAt(absCell, out _);
        if (hasTowerGO) { f |= CellFlags.Tower; }
        return f;
    }

    private Vector2Int AbsCellToRC(Vector3Int abs) { return new Vector2Int(abs.y - _origin.y, abs.x - _origin.x); }
    private Vector3Int RCToAbsCell(int r, int c) { return new Vector3Int(_origin.x + c, _origin.y + r, 0); }
}
