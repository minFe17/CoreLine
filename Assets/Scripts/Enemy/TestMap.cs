using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TestMap : MonoBehaviour
{
  
    [Flags]
    public enum CellFlags { None = 0, Wall = 1 << 0, Destructible = 1 << 1, Tower = 1 << 2 }

    public int Width { get; private set; }
    public int Height { get; private set; }
    public float CellSize { get; private set; }

    public CellFlags[,] cells { get; private set; }

    public event Action<int, int> OnCellChanged;

    public Vector2Int SpawnCellRC { get; private set; } = new Vector2Int(-1, -1);
    public bool HasSpawnCell => SpawnCellRC.x >= 0;

    private MapManager _map;
    private Vector3Int _origin;     
    private Vector3Int _sizeCells;  

    void Awake()
    {
        _map = MapManager.Instance;
        if (_map == null || !_map.IsReady) return;

        
        _map.GetNavFrame(out _origin, out _sizeCells, out Vector3 cellSize); 
        Width = _sizeCells.x;
        Height = _sizeCells.y;
        CellSize = cellSize.x;

        
        cells = new CellFlags[Height, Width];
        BuildSnapshotAll();

       
        if (_map.TryGetSpawnCell(out Vector3Int spawnAbs))
        {
            Vector2Int rc = AbsCellToRC(spawnAbs);
            if (InBounds(rc.x, rc.y)) SpawnCellRC = rc;
        }

        _map.OnCellChanged += HandleMapCellChanged;
    }

    void OnDestroy()
    {
        if (_map != null) _map.OnCellChanged -= HandleMapCellChanged;
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

    public bool InBounds(int r, int c) => (r >= 0 && c >= 0 && r < Height && c < Width);

    public bool IsWalkable(int r, int c)
    {
        if (!InBounds(r, c)) return false;

        if (_map != null && _map.HasPlayerBase)
        {
            Vector3Int abs = RCToAbsCell(r, c);
            if (abs == _map.PlayerBaseCell)
                return true;
        }

        CellFlags f = cells[r, c];
        return (f & (CellFlags.Wall | CellFlags.Destructible | CellFlags.Tower)) == 0;
    }

    public bool IsWall(int r, int c) => InBounds(r, c) && (cells[r, c] & CellFlags.Wall) != 0;
    public bool IsDestructible(int r, int c) => InBounds(r, c) && (cells[r, c] & CellFlags.Destructible) != 0;
    public bool HasTower(int r, int c) => InBounds(r, c) && (cells[r, c] & CellFlags.Tower) != 0;

    public bool TryGetSpawnCell(out Vector2Int rc)
    {
        rc = SpawnCellRC; return HasSpawnCell;
    }

    public void SetDestructible(int r, int c, bool on)
    {
        if (!InBounds(r, c)) return;
        Vector3Int abs = RCToAbsCell(r, c);
        if (!on)
            _map.ConvertDestructibleToBuildable(abs);
    }

    public void SetTower(int r, int c, bool on)
    {
        if (!InBounds(r, c)) return;
        Vector3Int abs = RCToAbsCell(r, c);
        if (on) 
            _map.MarkOccupied(abs);
        else 
            _map.UnmarkOccupied(abs);
    }

    private void BuildSnapshotAll()
    {
        for (int r = 0; r < Height; r++)
            for (int c = 0; c < Width; c++)
                cells[r, c] = SampleFlags(RCToAbsCell(r, c));
    }

    private void HandleMapCellChanged(Vector3Int absCell)
    {
        Vector2Int rc = AbsCellToRC(absCell);
        if (!InBounds(rc.x, rc.y)) return;

        cells[rc.x, rc.y] = SampleFlags(absCell);
        OnCellChanged?.Invoke(rc.x, rc.y);
    }

    private CellFlags SampleFlags(Vector3Int absCell)
    {
        if (_map != null && _map.HasPlayerBase && absCell == _map.PlayerBaseCell)
            return CellFlags.None;

        _map.GetCellFlags(absCell,
            out bool buildable, out bool unbuildable,
            out bool wall, out bool destructible, out bool deco, out bool occupied);

        CellFlags f = CellFlags.None;
        if (wall) f |= CellFlags.Wall;
        if (destructible) f |= CellFlags.Destructible;
        if (occupied) f |= CellFlags.Tower;  
        return f;
    }

    private Vector2Int AbsCellToRC(Vector3Int abs)
        => new Vector2Int(abs.y - _origin.y, abs.x - _origin.x); 

    private Vector3Int RCToAbsCell(int r, int c)
        => new Vector3Int(_origin.x + c, _origin.y + r, 0);
}
