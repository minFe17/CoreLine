using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TestMap : MonoBehaviour
{
    [Flags]
    public enum CellFlags
    {
        None = 0,
        Wall = 1 << 0,   // 파괴 불가 벽 (UnDeWall)
        Destructible = 1 << 1,   // 파괴 가능 벽 (DeWall)
        Tower = 1 << 2,   // 타워/점유 (게임 중 동적 표기)
    }

    [Header("Stage (Tilemap Root)")]
    [Tooltip("Realmap(Grid) 루트를 드래그하세요.")]
    public Transform stageRoot;

    [Header("Tilemap names (exact)")]
    public string buildName = "Build";
    public string unbuildName = "UnBuild";
    public string undeWallName = "UnDeWall";
    public string deWallName = "DeWall";
    public string decoName = "Decotile";

    // 내부 캐시
    Grid _grid;
    Tilemap _tmBuild, _tmUnbuild, _tmUnDeWall, _tmDeWall, _tmDeco;

    // 타일맵 전체 경계(월드타일 좌표)
    BoundsInt _bounds;

    // 외부 노출: 크기/셀크기 (CellToWorld는 Grid 기준으로 반환)
    public int Width { get; private set; }
    public int Height { get; private set; }
    public float CellSize => _grid ? _grid.cellSize.x : 1f;

    // 셀 상태
    public CellFlags[,] cells;

    // 셀 변경 알림 (r, c)
    public event Action<int, int> OnCellChanged;

    // ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (!stageRoot)
        {
            Debug.LogError("[TestMap] stageRoot(Realmap) 가 비어있습니다.");
            return;
        }
        CacheMapsFrom(stageRoot);
        RebuildFromTilemaps();
    }

    // 에디터에서 이름 바뀌었을 때 편하게 재적용
    void OnValidate()
    {
        if (stageRoot && Application.isPlaying == false)
        {
            CacheMapsFrom(stageRoot);
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Tilemap 캐시 + 경계 수집 + CellFlags 스캔
    // ─────────────────────────────────────────────────────────────

    void CacheMapsFrom(Transform root)
    {
        _grid = root.GetComponent<Grid>();
        if (!_grid)
        {
            Debug.LogError("[TestMap] Grid가 stageRoot에 없습니다.");
            return;
        }

        _tmBuild = FindByName(root, buildName)?.GetComponent<Tilemap>();
        _tmUnbuild = FindByName(root, unbuildName)?.GetComponent<Tilemap>();
        _tmUnDeWall = FindByName(root, undeWallName)?.GetComponent<Tilemap>();
        _tmDeWall = FindByName(root, deWallName)?.GetComponent<Tilemap>();
        _tmDeco = FindByName(root, decoName)?.GetComponent<Tilemap>();
    }

    public void RebuildFromTilemaps()
    {
        if (!_grid)
        {
            Debug.LogError("[TestMap] Grid가 없습니다. stageRoot를 확인하세요.");
            return;
        }

        _bounds = CalcUnionBounds(_tmBuild, _tmUnbuild, _tmUnDeWall, _tmDeWall);
        if (_bounds.size.x <= 0 || _bounds.size.y <= 0)
        {
            Debug.LogWarning("[TestMap] 유효한 타일맵 bounds가 없습니다.");
            Width = Height = 0;
            cells = null;
            return;
        }

        Width = _bounds.size.x;   // 열 c(x)
        Height = _bounds.size.y;  // 행 r(y)
        cells = new CellFlags[Height, Width];

        // 초기 스캔: 우선순위 Wall > Destructible > (기타=walkable)
        for (int dy = 0; dy < Height; dy++)
        {
            for (int dx = 0; dx < Width; dx++)
            {
                var xy = new Vector3Int(_bounds.xMin + dx, _bounds.yMin + dy, 0);

                if (_tmUnDeWall && _tmUnDeWall.HasTile(xy))
                {
                    cells[dy, dx] = CellFlags.Wall;
                }
                else if (_tmDeWall && _tmDeWall.HasTile(xy))
                {
                    cells[dy, dx] = CellFlags.Destructible;
                }
                else
                {
                    // Build/UnBuild/Deco는 이동 가능으로 취급
                    cells[dy, dx] = CellFlags.None;
                }
            }
        }

       
    }


    public Vector3 CellToWorld(int r, int c)
    {
        var cellXY = new Vector3Int(_bounds.xMin + c, _bounds.yMin + r, 0);
        return _grid.GetCellCenterWorld(cellXY); // 항상 중앙
    }

    public Vector2Int WorldToCell(Vector3 world)
    {
        var xy = _grid.WorldToCell(world);
        int c = xy.x - _bounds.xMin; // 열
        int r = xy.y - _bounds.yMin; // 행

        r = Mathf.Clamp(r, 0, Height - 1);
        c = Mathf.Clamp(c, 0, Width - 1);
        return new Vector2Int(r, c);
    }

    public bool InBounds(int r, int c) => cells != null && r >= 0 && c >= 0 && r < Height && c < Width;

    public bool IsWalkable(int r, int c)
    {
        if (!InBounds(r, c)) return false;
        var f = cells[r, c];
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
            var t = FindByName(root.GetChild(i), name);
            if (t) return t;
        }
        return null;
    }

    static BoundsInt CalcUnionBounds(params Tilemap[] maps)
    {
        bool any = false;
        BoundsInt b = new BoundsInt();
        foreach (var tm in maps)
        {
            if (!tm) continue;
            var tb = tm.cellBounds;
            if (!any) { b = tb; any = true; }
            else
            {
                var min = Vector3Int.Min(b.min, tb.min);
                var max = Vector3Int.Max(b.max, tb.max);
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
            var wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            wp.z = 0f;
            var rc = WorldToCell(wp);

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
