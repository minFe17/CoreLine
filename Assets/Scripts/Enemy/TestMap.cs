using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TestMap : MonoBehaviour
{
    //[Flags]
    //public enum CellFlags
    //{
    //    None = 0,
    //    Wall = 1 << 0,   // 벽
    //    Destructible = 1 << 1,   // 파괴 가능 벽 
    //    Tower = 1 << 2,   // 타워
    //}

    //[SerializeField] private Transform StageRoot;
    //private string _buildName = "Build";
    //private string _unbuildName = "UnBuild";
    //private string _undeWallName = "UnDeWall";
    //private string _deWallName = "DeWall";
    //private string _decoName = "Decotile";
    //private string _spawnName = "MonsterSpawnTile";


    //private Grid _grid;
    //private Tilemap _tmBuild, _tmUnbuild, _tmUnDeWall, _tmDeWall, _tmDeco, _tmSpawn;

    //private BoundsInt _bounds;

    //public int Width { get; private set; }
    //public int Height { get; private set; }
    //public float CellSize => _grid ? _grid.cellSize.x : 1f;
    //public CellFlags[,] cells;
    //public event Action<int, int> OnCellChanged;

    //public Vector2Int SpawnCellRC { get; private set; } = new Vector2Int(-1, -1); // (r,c) 저장
    //public bool HasSpawnCell => SpawnCellRC.x >= 0;

    //private void Awake()
    //{
    //    if (!StageRoot) return;

    //    CacheMapsFrom(StageRoot);
    //    RebuildFromTilemaps();
    //}


    //private void OnValidate()
    //{
    //    if (StageRoot && Application.isPlaying == false)
    //    {
    //        CacheMapsFrom(StageRoot);
    //    }
    //}


    //private void CacheMapsFrom(Transform root)
    //{
    //    _grid = root.GetComponent<Grid>();
    //    if (!_grid)
    //    {
    //        Debug.LogError("[TestMap] Grid가 stageRoot에 없습니다.");
    //        return;
    //    }

    //    _tmBuild = FindByName(root, _buildName)?.GetComponent<Tilemap>();
    //    _tmUnbuild = FindByName(root, _unbuildName)?.GetComponent<Tilemap>();
    //    _tmUnDeWall = FindByName(root, _undeWallName)?.GetComponent<Tilemap>();
    //    _tmDeWall = FindByName(root, _deWallName)?.GetComponent<Tilemap>();
    //    _tmDeco = FindByName(root, _decoName)?.GetComponent<Tilemap>();
    //    _tmSpawn = FindByName(root, _spawnName)?.GetComponent<Tilemap>();
    //}

    //public void RebuildFromTilemaps()
    //{
    //    if (!_grid)
    //        return;

    //    _bounds = CalcUnionBounds(_tmBuild, _tmUnbuild, _tmUnDeWall, _tmDeWall, _tmSpawn);
    //    if (_bounds.size.x <= 0 || _bounds.size.y <= 0)
    //    {
    //        Width = Height = 0;
    //        cells = null;
    //        SpawnCellRC = new Vector2Int(-1, -1);
    //        return;
    //    }

    //    Width = _bounds.size.x;
    //    Height = _bounds.size.y;
    //    cells = new CellFlags[Height, Width];

    //    for (int dy = 0; dy < Height; dy++)
    //    {
    //        for (int dx = 0; dx < Width; dx++)
    //        {
    //            Vector3Int xy = new Vector3Int(_bounds.xMin + dx, _bounds.yMin + dy, 0);

    //            if (_tmUnDeWall && _tmUnDeWall.HasTile(xy))
    //                cells[dy, dx] = CellFlags.Wall;
    //            else if (_tmDeWall && _tmDeWall.HasTile(xy))
    //                cells[dy, dx] = CellFlags.Destructible;
    //            else
    //                cells[dy, dx] = CellFlags.None;
    //        }
    //    }

    //    SpawnCellRC = new Vector2Int(-1, -1);
    //    if (_tmSpawn)
    //    {
    //        BoundsInt sb = _tmSpawn.cellBounds;
    //        foreach (var pos in sb.allPositionsWithin)
    //        {
    //            if (!_tmSpawn.HasTile(pos)) continue;
    //            int c = pos.x - _bounds.xMin;
    //            int r = pos.y - _bounds.yMin;
    //            if (InBounds(r, c))
    //            {
    //                SpawnCellRC = new Vector2Int(r, c);
    //                break;
    //            }
    //        }
    //    }

    //}
    //public bool TryGetSpawnCell(out Vector2Int rc)
    //{
    //    rc = SpawnCellRC;
    //    return HasSpawnCell;
    //}

    //public Vector3 CellToWorld(int r, int c)
    //{
    //    Vector3Int cellXY = new Vector3Int(_bounds.xMin + c, _bounds.yMin + r, 0);
    //    return _grid.GetCellCenterWorld(cellXY);
    //}

    //public Vector2Int WorldToCell(Vector3 world)
    //{
    //    Vector3Int xy = _grid.WorldToCell(world);
    //    int c = xy.x - _bounds.xMin;
    //    int r = xy.y - _bounds.yMin;

    //    r = Mathf.Clamp(r, 0, Height - 1);
    //    c = Mathf.Clamp(c, 0, Width - 1);
    //    return new Vector2Int(r, c);
    //}

    //public bool InBounds(int r, int c) => cells != null && r >= 0 && c >= 0 && r < Height && c < Width;

    //public bool IsWalkable(int r, int c)
    //{
    //    if (!InBounds(r, c)) return false;
    //    CellFlags f = cells[r, c];
    //    return (f & (CellFlags.Wall | CellFlags.Destructible | CellFlags.Tower)) == 0;
    //}

    //public bool IsWall(int r, int c) => InBounds(r, c) && (cells[r, c] & CellFlags.Wall) != 0;
    //public bool IsDestructible(int r, int c) => InBounds(r, c) && (cells[r, c] & CellFlags.Destructible) != 0;
    //public bool HasTower(int r, int c) => InBounds(r, c) && (cells[r, c] & CellFlags.Tower) != 0;

    //public void SetWall(int r, int c, bool on)
    //{
    //    if (!InBounds(r, c)) return;
    //    if (on) cells[r, c] |= CellFlags.Wall; else cells[r, c] &= ~CellFlags.Wall;
    //    OnCellChanged?.Invoke(r, c);
    //}

    //public void SetDestructible(int r, int c, bool on)
    //{
    //    if (!InBounds(r, c)) return;
    //    if (on) cells[r, c] |= CellFlags.Destructible; else cells[r, c] &= ~CellFlags.Destructible;
    //    OnCellChanged?.Invoke(r, c);
    //}

    //public void SetTower(int r, int c, bool on)
    //{
    //    if (!InBounds(r, c)) return;
    //    if (on) cells[r, c] |= CellFlags.Tower; else cells[r, c] &= ~CellFlags.Tower;
    //    OnCellChanged?.Invoke(r, c);
    //}



    //static Transform FindByName(Transform root, string name)
    //{
    //    if (root == null || string.IsNullOrEmpty(name)) return null;
    //    if (root.name == name) return root;
    //    for (int i = 0; i < root.childCount; i++)
    //    {
    //        Transform t = FindByName(root.GetChild(i), name);
    //        if (t) return t;
    //    }
    //    return null;
    //}

    //static BoundsInt CalcUnionBounds(params Tilemap[] maps)
    //{
    //    bool any = false;
    //    BoundsInt b = new BoundsInt();
    //    foreach (Tilemap tm in maps)
    //    {
    //        if (!tm) continue;
    //        BoundsInt tb = tm.cellBounds;
    //        if (!any) { b = tb; any = true; }
    //        else
    //        {
    //            Vector3Int min = Vector3Int.Min(b.min, tb.min);
    //            Vector3Int max = Vector3Int.Max(b.max, tb.max);
    //            b = new BoundsInt(min, max - min);
    //        }
    //    }
    //    return b;
    //}


    //public enum PaintMode { None, Wall, Destructible, Tower, Clear }
    //[Header("Editor Paint (optional)")]
    //public bool editorPaint = false;
    //public PaintMode paintMode = PaintMode.None;

    //void Update()
    //{
    //    if (!editorPaint) return;
    //    if (!Camera.main) return;

    //    if (Input.GetMouseButtonDown(0))
    //    {
    //        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //        wp.z = 0f;
    //        Vector2Int rc = WorldToCell(wp);

    //        switch (paintMode)
    //        {
    //            case PaintMode.Wall: SetWall(rc.x, rc.y, true); SetDestructible(rc.x, rc.y, false); SetTower(rc.x, rc.y, false); break;
    //            case PaintMode.Destructible: SetWall(rc.x, rc.y, false); SetDestructible(rc.x, rc.y, true); SetTower(rc.x, rc.y, false); break;
    //            case PaintMode.Tower: SetWall(rc.x, rc.y, false); SetDestructible(rc.x, rc.y, false); SetTower(rc.x, rc.y, true); break;
    //            case PaintMode.Clear: SetWall(rc.x, rc.y, false); SetDestructible(rc.x, rc.y, false); SetTower(rc.x, rc.y, false); break;
    //        }
    //    }
    //}

    [Flags]
    public enum CellFlags { None = 0, Wall = 1 << 0, Destructible = 1 << 1, Tower = 1 << 2 }

    public int Width { get; private set; }
    public int Height { get; private set; }
    public float CellSize { get; private set; }

    // GridVisualizer 등 호환용 스냅샷(필수는 아님). OnCellChanged에서 부분 갱신.
    public CellFlags[,] cells { get; private set; }

    // 바깥 코드와 시그니처 맞추기( r,c )
    public event Action<int, int> OnCellChanged;

    // (r,c)로 저장
    public Vector2Int SpawnCellRC { get; private set; } = new Vector2Int(-1, -1);
    public bool HasSpawnCell => SpawnCellRC.x >= 0;

    private MapManager _map;
    private Vector3Int _origin;     // 맵 원점(절대 셀 좌표)
    private Vector3Int _sizeCells;  // 맵 크기(절대 셀 좌표계에서 x=Width, y=Height)

    void Awake()
    {
        _map = MapManager.Instance;
        if (_map == null || !_map.IsReady) return;

        // 프레임/셀 크기 얻기
        _map.GetNavFrame(out _origin, out _sizeCells, out var cellSize); // (origin, size, cellSize)
        Width = _sizeCells.x;
        Height = _sizeCells.y;
        CellSize = cellSize.x;

        // 스냅샷 한 번 구성
        cells = new CellFlags[Height, Width];
        BuildSnapshotAll();

        // 스폰셀 캐시
        if (_map.TryGetSpawnCell(out var spawnAbs))
        {
            var rc = AbsCellToRC(spawnAbs);
            if (InBounds(rc.x, rc.y)) SpawnCellRC = rc;
        }

        // 변경 이벤트 중계(절대 셀 → r,c)
        _map.OnCellChanged += HandleMapCellChanged;
    }

    void OnDestroy()
    {
        if (_map != null) _map.OnCellChanged -= HandleMapCellChanged;
    }

    // ── 좌표 변환: 월드/절대 셀/로컬(r,c) ─────────────────────────────

    public Vector2Int WorldToCell(Vector3 world)
    {
        var abs = _map.WorldToCell(world);                     // 절대 셀
        var rc = AbsCellToRC(abs);                            // r,c
        // Clamp
        rc.x = Mathf.Clamp(rc.x, 0, Height - 1);
        rc.y = Mathf.Clamp(rc.y, 0, Width - 1);
        return rc;
    }

    public Vector3 CellToWorld(int r, int c)
    {
        var abs = RCToAbsCell(r, c);
        return _map.CellCenterWorld(abs);
    }

    // ── 질의 API(기존 시그니처 유지) ──────────────────────────────────

    public bool InBounds(int r, int c) => (r >= 0 && c >= 0 && r < Height && c < Width);

    public bool IsWalkable(int r, int c)
    {
        if (!InBounds(r, c)) return false;
        // 캐시 기준(경량). 더 정확히 하려면 _map.GetNavInfo(abs) 직접 조회.
        var f = cells[r, c];
        return (f & (CellFlags.Wall | CellFlags.Destructible | CellFlags.Tower)) == 0;
    }

    public bool IsWall(int r, int c) => InBounds(r, c) && (cells[r, c] & CellFlags.Wall) != 0;
    public bool IsDestructible(int r, int c) => InBounds(r, c) && (cells[r, c] & CellFlags.Destructible) != 0;
    public bool HasTower(int r, int c) => InBounds(r, c) && (cells[r, c] & CellFlags.Tower) != 0;

    public bool TryGetSpawnCell(out Vector2Int rc)
    {
        rc = SpawnCellRC; return HasSpawnCell;
    }

    // ── 변경(호출부 최소 대응): 파괴벽 제거/타워 점유(옵션) ─────────────

    // 기존 MonsterMover는 SetDestructible(r,c,false)만 사용 → MapManager로 위임
    public void SetDestructible(int r, int c, bool on)
    {
        if (!InBounds(r, c)) return;
        var abs = RCToAbsCell(r, c);
        if (!on)
        {
            // 파괴 가능 벽을 Buildable로 전환 + OnCellChanged 발행
            _map.ConvertDestructibleToBuildable(abs);
        }
        else
        {
            // (필요 시 구현) 에디터 페인트 등에서 되돌리기를 원하면 별도 타일 세팅 로직을 MapManager 측에 확장
            Debug.LogWarning("[TestMapCompat] SetDestructible(true)는 미구현입니다.");
        }
    }

    // 에디터용/간이 점유. 실제 타워 설치는 RegisterTower(prefab) 사용 권장.
    public void SetTower(int r, int c, bool on)
    {
        if (!InBounds(r, c)) return;
        var abs = RCToAbsCell(r, c);
        if (on) _map.MarkOccupied(abs);
        else _map.UnmarkOccupied(abs);
    }

    public void SetWall(int r, int c, bool on)
    {
        Debug.LogWarning("[TestMapCompat] SetWall는 런타임 미구현(필요시 MapManager 쪽 타일 세팅 확장).");
    }

    // ── 내부: 스냅샷/이벤트 갱신 ───────────────────────────────────────

    private void BuildSnapshotAll()
    {
        for (int r = 0; r < Height; r++)
            for (int c = 0; c < Width; c++)
                cells[r, c] = SampleFlags(RCToAbsCell(r, c));
    }

    private void HandleMapCellChanged(Vector3Int absCell)
    {
        var rc = AbsCellToRC(absCell);
        if (!InBounds(rc.x, rc.y)) return;

        cells[rc.x, rc.y] = SampleFlags(absCell);
        OnCellChanged?.Invoke(rc.x, rc.y);
    }

    private CellFlags SampleFlags(Vector3Int absCell)
    {
        _map.GetCellFlags(absCell,
            out bool buildable, out bool unbuildable,
            out bool wall, out bool destructible, out bool deco, out bool occupied);

        CellFlags f = CellFlags.None;
        if (wall) f |= CellFlags.Wall;
        if (destructible) f |= CellFlags.Destructible;
        if (occupied) f |= CellFlags.Tower;  // 타워/점유를 동일 취급(호환성 목적)
        return f;
    }

    private Vector2Int AbsCellToRC(Vector3Int abs)
        => new Vector2Int(abs.y - _origin.y, abs.x - _origin.x); // r=Y, c=X

    private Vector3Int RCToAbsCell(int r, int c)
        => new Vector3Int(_origin.x + c, _origin.y + r, 0);
}
