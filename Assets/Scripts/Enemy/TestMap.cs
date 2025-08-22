using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TestMap : MonoBehaviour
{
    [Flags]
    public enum CellFlags
    {
        None = 0,
        Wall = 1 << 0,   // 벽
        Destructible = 1 << 1,   // 파괴 가능 벽 
        Tower = 1 << 2,   // 타워
    }

    [SerializeField] private Transform StageRoot;
    private string _buildName = "Build";
    private string _unbuildName = "UnBuild";
    private string _undeWallName = "UnDeWall";
    private string _deWallName = "DeWall";
    private string _decoName = "Decotile";
    private string _spawnName = "MonsterSpawnTile";

   
    private Grid _grid;
    private Tilemap _tmBuild, _tmUnbuild, _tmUnDeWall, _tmDeWall, _tmDeco, _tmSpawn;

    private BoundsInt _bounds;

    public int Width { get; private set; }
    public int Height { get; private set; }
    public float CellSize => _grid ? _grid.cellSize.x : 1f;
    public CellFlags[,] cells;
    public event Action<int, int> OnCellChanged;

    public Vector2Int SpawnCellRC { get; private set; } = new Vector2Int(-1, -1); // (r,c) 저장
    public bool HasSpawnCell => SpawnCellRC.x >= 0;

    private void Awake()
    {
        if (!StageRoot) return;

        CacheMapsFrom(StageRoot);
        RebuildFromTilemaps();
    }


    private void OnValidate()
    {
        if (StageRoot && Application.isPlaying == false)
        {
            CacheMapsFrom(StageRoot);
        }
    }


    private void CacheMapsFrom(Transform root)
    {
        _grid = root.GetComponent<Grid>();
        if (!_grid)
        {
            Debug.LogError("[TestMap] Grid가 stageRoot에 없습니다.");
            return;
        }

        _tmBuild = FindByName(root, _buildName)?.GetComponent<Tilemap>();
        _tmUnbuild = FindByName(root, _unbuildName)?.GetComponent<Tilemap>();
        _tmUnDeWall = FindByName(root, _undeWallName)?.GetComponent<Tilemap>();
        _tmDeWall = FindByName(root, _deWallName)?.GetComponent<Tilemap>();
        _tmDeco = FindByName(root, _decoName)?.GetComponent<Tilemap>();
        _tmSpawn = FindByName(root, _spawnName)?.GetComponent<Tilemap>();
    }

    public void RebuildFromTilemaps()
    {
        if (!_grid)
            return;

        _bounds = CalcUnionBounds(_tmBuild, _tmUnbuild, _tmUnDeWall, _tmDeWall, _tmSpawn);
        if (_bounds.size.x <= 0 || _bounds.size.y <= 0)
        {
            Width = Height = 0;
            cells = null;
            SpawnCellRC = new Vector2Int(-1, -1);
            return;
        }

        Width = _bounds.size.x;   
        Height = _bounds.size.y;  
        cells = new CellFlags[Height, Width];

        for (int dy = 0; dy < Height; dy++)
        {
            for (int dx = 0; dx < Width; dx++)
            {
                Vector3Int xy = new Vector3Int(_bounds.xMin + dx, _bounds.yMin + dy, 0);

                if (_tmUnDeWall && _tmUnDeWall.HasTile(xy))
                    cells[dy, dx] = CellFlags.Wall;
                else if (_tmDeWall && _tmDeWall.HasTile(xy))
                    cells[dy, dx] = CellFlags.Destructible;
                else
                    cells[dy, dx] = CellFlags.None;
            }
        }

        SpawnCellRC = new Vector2Int(-1, -1);
        if (_tmSpawn)
        {
            BoundsInt sb = _tmSpawn.cellBounds;
            foreach (var pos in sb.allPositionsWithin)
            {
                if (!_tmSpawn.HasTile(pos)) continue;
                int c = pos.x - _bounds.xMin;
                int r = pos.y - _bounds.yMin;
                if (InBounds(r, c))
                {
                    SpawnCellRC = new Vector2Int(r, c);
                    break;
                }
            }
        }

    }
    public bool TryGetSpawnCell(out Vector2Int rc)
    {
        rc = SpawnCellRC;
        return HasSpawnCell;
    }

    public Vector3 CellToWorld(int r, int c)
    {
        Vector3Int cellXY = new Vector3Int(_bounds.xMin + c, _bounds.yMin + r, 0);
        return _grid.GetCellCenterWorld(cellXY);
    }

    public Vector2Int WorldToCell(Vector3 world)
    {
        Vector3Int xy = _grid.WorldToCell(world);
        int c = xy.x - _bounds.xMin; 
        int r = xy.y - _bounds.yMin;

        r = Mathf.Clamp(r, 0, Height - 1);
        c = Mathf.Clamp(c, 0, Width - 1);
        return new Vector2Int(r, c);
    }

    public bool InBounds(int r, int c) => cells != null && r >= 0 && c >= 0 && r < Height && c < Width;

    public bool IsWalkable(int r, int c)
    {
        if (!InBounds(r, c)) return false;
        CellFlags f = cells[r, c];
        return (f & (CellFlags.Wall | CellFlags.Destructible | CellFlags.Tower)) == 0;
    }

    public bool IsWall(int r, int c) => InBounds(r, c) && (cells[r, c] & CellFlags.Wall) != 0;
    public bool IsDestructible(int r, int c) => InBounds(r, c) && (cells[r, c] & CellFlags.Destructible) != 0;
    public bool HasTower(int r, int c) => InBounds(r, c) && (cells[r, c] & CellFlags.Tower) != 0;

    public void SetWall(int r, int c, bool on)
    {
        if (!InBounds(r, c)) return;
        if (on) cells[r, c] |= CellFlags.Wall; else cells[r, c] &= ~CellFlags.Wall;
        OnCellChanged?.Invoke(r, c);
    }

    public void SetDestructible(int r, int c, bool on)
    {
        if (!InBounds(r, c)) return;
        if (on) cells[r, c] |= CellFlags.Destructible; else cells[r, c] &= ~CellFlags.Destructible;
        OnCellChanged?.Invoke(r, c);
    }

    public void SetTower(int r, int c, bool on)
    {
        if (!InBounds(r, c)) return;
        if (on) cells[r, c] |= CellFlags.Tower; else cells[r, c] &= ~CellFlags.Tower;
        OnCellChanged?.Invoke(r, c);
    }

    

    static Transform FindByName(Transform root, string name)
    {
        if (root == null || string.IsNullOrEmpty(name)) return null;
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform t = FindByName(root.GetChild(i), name);
            if (t) return t;
        }
        return null;
    }

    static BoundsInt CalcUnionBounds(params Tilemap[] maps)
    {
        bool any = false;
        BoundsInt b = new BoundsInt();
        foreach (Tilemap tm in maps)
        {
            if (!tm) continue;
            BoundsInt tb = tm.cellBounds;
            if (!any) { b = tb; any = true; }
            else
            {
                Vector3Int min = Vector3Int.Min(b.min, tb.min);
                Vector3Int max = Vector3Int.Max(b.max, tb.max);
                b = new BoundsInt(min, max - min);
            }
        }
        return b;
    }

    
    public enum PaintMode { None, Wall, Destructible, Tower, Clear }
    [Header("Editor Paint (optional)")]
    public bool editorPaint = false;
    public PaintMode paintMode = PaintMode.None;

    void Update()
    {
        if (!editorPaint) return;
        if (!Camera.main) return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            wp.z = 0f;
            Vector2Int rc = WorldToCell(wp);

            switch (paintMode)
            {
                case PaintMode.Wall: SetWall(rc.x, rc.y, true); SetDestructible(rc.x, rc.y, false); SetTower(rc.x, rc.y, false); break;
                case PaintMode.Destructible: SetWall(rc.x, rc.y, false); SetDestructible(rc.x, rc.y, true); SetTower(rc.x, rc.y, false); break;
                case PaintMode.Tower: SetWall(rc.x, rc.y, false); SetDestructible(rc.x, rc.y, false); SetTower(rc.x, rc.y, true); break;
                case PaintMode.Clear: SetWall(rc.x, rc.y, false); SetDestructible(rc.x, rc.y, false); SetTower(rc.x, rc.y, false); break;
            }
        }
    }
}
