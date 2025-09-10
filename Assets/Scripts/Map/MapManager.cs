using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Threading.Tasks;
public class MapManager : MonoBehaviour
{
    public static MapManager Instance => Utils.MonoSingleton<MapManager>.Instance;

    private bool _hasSpawn;
    private Vector3Int _spawnCell;

    private bool _hasBossSpawn;
    private Vector3Int _bossSpawnCell;

    private GameObject _stageRoot;
    private Grid _grid;
    private Tilemap _tmBuildable, _tmUnbuildable, _tmWall, _tmDestructible, 
                    _tmDeco, _tmKing, _tmObjects, _tmMonsterSpawn, _tmBossSpawn;

    private readonly HashSet<Vector3Int> _occupied = new();
    private readonly Dictionary<Vector3Int, GameObject> _towers = new(); // ¼¿ ¡æ Å¸¿ö ¿ÀºêÁ§Æ®
    public bool IsReady => _grid != null;
    private bool _hasPlayerBase;
    private Vector3Int _playerBaseCell;
    private Transform _objectsRoot;
    private readonly HashSet<Vector3Int> _objectCells = new();

    public bool HasPlayerBase => _hasPlayerBase;
    public Vector3Int PlayerBaseCell => _playerBaseCell;
    public Vector3 PlayerBaseWorld => IsReady && _hasPlayerBase ? CellCenterWorld(_playerBaseCell) : Vector3.zero;
    // ³×ºñ/¹èÄ¡ º¯°æ ¾Ë¸²(Å¸¿ö ¼³Ä¡/Á¦°Å, ÆÄ±«º® º¯°æ µî)
    public Action<Vector3Int> OnCellChanged;
    public event Action<Vector3Int> OnPlayerBasePlaced; // ÇÃ·¹ÀÌ¾î º£ÀÌ½º°¡ ¹èÄ¡µÆÀ» ¶§
    public bool HasBossSpawn => _hasBossSpawn;
    public Vector3Int BossSpawnCell => _bossSpawnCell;
    public Vector3 BossSpawnWorld => (IsReady && _hasBossSpawn) ? CellCenterWorld(_bossSpawnCell) : Vector3.zero;
    public Tilemap BuildableTile { get => _tmBuildable; }
    public Tilemap UnbuildableTile { get => _tmUnbuildable; }

    //¸Ê ·Îµå½Ã ¹Ù·Î ¼ÒÈ¯ÇÏ°í ½ÍÀ» ¶§
    public event Action<Vector3Int> OnBossSpawnFound;
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ½ºÅ×ÀÌÁö ·Îµå/¹ÙÀÎµå/¾ð·Îµå
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void Update()
    {
        if (Input.GetMouseButtonDown(2)) // ÁßÅ¬¸¯ Å×½ºÆ®
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f; // 2D´Ï±î z °íÁ¤
            DebugCheckTowerPlace(mouseWorld);
        }
    }

    public void LoadStage(GameObject stagePrefab)
    {
        UnloadStage();
        _stageRoot = Instantiate(stagePrefab);
        _stageRoot.name = stagePrefab.name;

        CacheMapsFrom(_stageRoot.transform);
        //SetupCollisionLayers();
        WireDestructibleController();
        InitSpawnCell();
        InitBossSpawnCell();
    }

    public void BindStageRoot(Transform stageRoot)
    {
        UnloadStage();
        _stageRoot = stageRoot.gameObject;

        CacheMapsFrom(stageRoot);
        //SetupCollisionLayers();
        WireDestructibleController();
        InitSpawnCell();
        InitBossSpawnCell();
    }

    public void UnloadStage()
    {
        _occupied.Clear();
        _towers.Clear();

        _hasPlayerBase = false;
        _playerBaseCell = default;
        _hasSpawn = false;
        _spawnCell = default;
        _hasBossSpawn = false;
        _bossSpawnCell = default;

        _grid = null;
        _tmBuildable = _tmUnbuildable = _tmWall = _tmDestructible = _tmDeco = null;

        if (_stageRoot != null)
        {
            Destroy(_stageRoot);
            _stageRoot = null;
        }
    }
    private void InitSpawnCell()
    {
        _hasSpawn = false;
        if (_tmMonsterSpawn == null) return;

        foreach (Vector3Int cell in _tmMonsterSpawn.cellBounds.allPositionsWithin)
        {
            if (_tmMonsterSpawn.HasTile(cell))
            {
                if (_hasSpawn)
                {
                    Debug.LogWarning($"[MapManager] Spawn Å¸ÀÏÀÌ ¿©·¯ °³ÀÔ´Ï´Ù. Ã¹ ¹øÂ°({_spawnCell})¸¸ »ç¿ë, ³ª¸ÓÁö {cell} ¹«½Ã.");
                    break; // Ã¹ ¹øÂ°¸¸ ¾²°í ¹Ù·Î Á¾·á
                }

                _spawnCell = cell;
                _hasSpawn = true;
                break; // Ã¹ ¹øÂ° Ã£¾ÒÀ¸¸é ¹Ù·Î Á¾·á
            }
        }
    }

    private void InitBossSpawnCell()
    {
        _hasBossSpawn = false;
        _bossSpawnCell = default;
        if (_tmBossSpawn == null) return;

        foreach (Vector3Int cell in _tmBossSpawn.cellBounds.allPositionsWithin)
        {
            if (_tmBossSpawn.HasTile(cell))
            {
                if (_hasBossSpawn)
                {
                    Debug.LogWarning($"[MapManager] BossSpawn Å¸ÀÏÀÌ ¿©·¯ °³ÀÔ´Ï´Ù. Ã¹ ¹øÂ°({_bossSpawnCell})¸¸ »ç¿ë, {cell} ¹«½Ã.");
                    break; // Ã¹ ¹øÂ°¸¸ ¾²°í ¹Ù·Î Á¾·á
                }

                _bossSpawnCell = cell;
                _hasBossSpawn = true;
                break; // Ã¹ ¹øÂ° Ã£¾ÒÀ¸¸é ¹Ù·Î Á¾·á
            }
        }

        if (_hasBossSpawn)
            OnBossSpawnFound?.Invoke(_bossSpawnCell);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÁÂÇ¥ À¯Æ¿
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public Vector3Int WorldToCell(Vector3 world)
    {
        if (!IsReady) { Debug.LogError("[MapManager] Stage not ready."); return Vector3Int.zero; }
        return _grid.WorldToCell(world);
    }

    public Vector3 CellCenterWorld(Vector3Int cell)
    {
        if (!IsReady) { Debug.LogError("[MapManager] Stage not ready."); return Vector3.zero; }
        return _grid.GetCellCenterWorld(cell);
    }

    public Vector3 GetCellCenterWorld(Vector3Int cell)
    {
        if (!IsReady) { Debug.LogError("[MapManager] Stage not ready."); return Vector3.zero; }
        return _grid.GetCellCenterWorld(cell);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // °æ·® ±¸Á¶Ã¼ & ÆíÀÇ API (¹èÄ¡/±æÃ£±â Àü¿ë)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    public readonly struct PlaceInfo
    {
        public readonly Vector3Int Cell;
        public readonly bool Placeable; // Áö±Ý Áï½Ã ¼³Ä¡ °¡´ÉÇÑ°¡?
        public readonly bool Occupied;  // Á¡À¯µÅ ÀÖ´Â°¡?

        public PlaceInfo(Vector3Int cell, bool placeable, bool occupied)
        { this.Cell = cell; this.Placeable = placeable; this.Occupied = occupied; }
    }
    public PlaceInfo GetPlaceInfo(Vector3Int cell)
    {
        bool occupied = _towers.ContainsKey(cell) || _occupied.Contains(cell);
        bool placeable =
            (_tmBuildable && _tmBuildable.HasTile(cell)) &&
            !occupied &&
            !((_tmWall && _tmWall.HasTile(cell)) || (_tmDestructible && _tmDestructible.HasTile(cell)));

        if (_hasPlayerBase && cell == _playerBaseCell)
            placeable = false;

        return new PlaceInfo(cell, placeable, occupied);
    }
    public PlaceInfo GetPlaceInfoWorld(Vector3 worldPos) => GetPlaceInfo(WorldToCell(worldPos));
    // ½ºÅ×ÀÌÁö ÁøÀÔ ½Ã ÇÃ·¹ÀÌ¾î º£ÀÌ½º ¹èÄ¡ °¡´É Å¸ÀÏ Get¿ë
    // ÇöÀç ¸ÊÀÇ ¸ðµç KingTile ¼¿ ³ª¿­
    public List<Vector3Int> GetAllKingCells()
    {
        List<Vector3Int> list = new List<Vector3Int>();
        if (_tmKing == null) return list;
        foreach (Vector3Int cell in _tmKing.cellBounds.allPositionsWithin)
            if (_tmKing.HasTile(cell)) list.Add(cell);
        return list;
    }

    // ¼±ÅÃµÈ KingTileÀ» Buildable·Î ÀüÈ¯
    public bool ConvertKingToBuildable(Vector3Int cell)
    {
        if (_tmKing == null || !_tmKing.HasTile(cell)) return false;
        _tmKing.SetTile(cell, null);
        OnCellChanged?.Invoke(cell);  // ±æÃ£±â/¹èÄ¡ °»½Å ¾Ë¸²
        return true;
    }
    public bool SelectPlayerBase(Vector3Int selectedCell, GameObject basePrefab = null, bool occupyBaseCell = true)
    {
        if (_tmKing == null) return false;
        if (!_tmKing.HasTile(selectedCell)) return false;

        List<Vector3Int> kings = GetAllKingCells();
        if (kings.Count == 0) return false;

        foreach (Vector3Int cell in kings)
        {
            _tmKing.SetTile(cell, null);

            if (cell == selectedCell)
            {
                if (basePrefab != null)
                {
                    Vector3 pos = CellCenterWorld(cell);
                    GameObject gameObject = Instantiate(basePrefab, pos, Quaternion.identity, _stageRoot?.transform);
                    gameObject.name = basePrefab.name;
                }

                if (occupyBaseCell)
                    MarkOccupied(cell);        // ¼³Ä¡ Á¦ÇÑÀº À¯ÁöÇÏ°í

                _playerBaseCell = cell;       // ¡Ú ¸ñÀûÁö·Î ¾µ ¼¿ ÀúÀå
                _hasPlayerBase = true;
            }

            OnCellChanged?.Invoke(cell);      // (ÀÖ´ø) ³×ºñ °»½ÅÀº ±×´ë·Î
        }

        if (_hasPlayerBase)
            OnPlayerBasePlaced?.Invoke(_playerBaseCell); // ÇÃ·¹ÀÌ¾î º£ÀÌ½º ¹èÄ¡ ¾Ë¸²

        return true;
    }

    public readonly struct NavInfo
    {
        public readonly Vector3Int Cell;
        public readonly bool Blocked;        // ÀÌµ¿ ºÒ°¡ ÀüÃ¼ ÆÇ´Ü
        public readonly bool BlockedByTower; // Å¸¿ö/Á¡À¯·Î ÀÎÇØ ¸·Èû
        public readonly bool BlockedByWall;  // º®/ÆÄ±«º®À¸·Î ¸·Èû

        public NavInfo(Vector3Int cell, bool blocked, bool blockedByTower, bool blockedByWall)
        { this.Cell = cell; this.Blocked = blocked; this.BlockedByTower = blockedByTower; this.BlockedByWall = blockedByWall; }
    }
    public NavInfo GetNavInfo(Vector3Int cell)
    {
        bool byWall = (_tmWall && _tmWall.HasTile(cell)) || (_tmDestructible && _tmDestructible.HasTile(cell));
        bool byTower = _towers.ContainsKey(cell) || _occupied.Contains(cell);
        bool blocked = byWall || byTower;
        return new NavInfo(cell, blocked, byTower, byWall);
    }
    public NavInfo GetNavInfoWorld(Vector3 worldPos) => GetNavInfo(WorldToCell(worldPos));

    //½Ç¼ö ¹æÁö
    public bool TryGetSpawnCell(out Vector3Int cell)
    {
        cell = _spawnCell;
        return _hasSpawn;
    }
    //¸ó½ºÅÍ ½ºÆù Å¸ÀÏ Get¿ë
    public Vector3 GetSpawnWorld()
    {
        return _hasSpawn ? CellCenterWorld(_spawnCell) : Vector3.zero;
    }
    public bool TryGetBossSpawnCell(out Vector3Int cell)
    {
        cell = _bossSpawnCell;
        return _hasBossSpawn;
    }
    public Vector3 GetBossSpawnWorld()
    {
        return _hasBossSpawn? CellCenterWorld(_bossSpawnCell) : Vector3.zero;
    }
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ½ºÅ×ÀÌÁö ·ÎµåÇÏ°í ¹Ù·Î º¸½º ¼ÒÈ¯½Ã
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // MapManager.Instance.OnBossSpawnFound += cell =>
    // {
    //     Vector3 pos = MapManager.Instance.CellCenterWorld(cell);
    //     Instantiate(bossPrefab, pos, Quaternion.identity);
    // };

    //¸ó½ºÅÍ°¡ Å¸¿ö °ø°Ý ÇÏ´Â ¿ë
    public bool TryGetTowerAt(Vector3Int cell, out GameObject tower) => _towers.TryGetValue(cell, out tower);
    
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¹èÄ¡/Á¡À¯ °»½Å
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void MarkOccupied(Vector3Int tile)
    {
        _occupied.Add(tile);
        OnCellChanged?.Invoke(tile);
    }

    public void UnmarkOccupied(Vector3Int tile)
    {
        _occupied.Remove(tile);
        OnCellChanged?.Invoke(tile);
    }

    // ¼³Ä¡ Á÷ÈÄ È£Ãâ: RegisterTower(cell, towerInstance);
    public void RegisterTower(Vector3Int cell, GameObject tower)
    {
        _towers[cell] = tower;
        _occupied.Add(cell);              // Á¡À¯ Ç¥±â
        OnCellChanged?.Invoke(cell);
    }

    // ÆÄ±«/ÆÇ¸Å ½Ã È£Ãâ: UnregisterTower(cell);
    public void UnregisterTower(Vector3Int cell)
    {
        _towers.Remove(cell);
        _occupied.Remove(cell);
        OnCellChanged?.Invoke(cell);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÆÄ±«º® Ã³¸®
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void ConvertDestructibleToBuildable(Vector3Int cell)
    {
        if (!IsReady || _tmDestructible == null) return;
        if (_tmDestructible.HasTile(cell))
        {
            _tmDestructible.SetTile(cell, null); // ¹ØÀÇ BuildableÀÌ ±×´ë·Î µå·¯³²
            //_tmDestructible.GetComponent<TilemapCollider2D>()?.ProcessTilemapChanges();
            OnCellChanged?.Invoke(cell);
        }
    }
    public bool IsDestructible(Vector3Int cell)
    {
        return _tmDestructible && _tmDestructible.HasTile(cell);
    }

    // ¿ùµå ÁÂÇ¥·Î ÆÇÁ¤
    public bool IsDestructibleWorld(Vector3 worldPos)
    {
        return IsDestructible(WorldToCell(worldPos));
    }

    // ÆÄ±«(= DeWall ¡æ Á¦°Å, ¾Æ·¡ Buildable µå·¯³²). ±âÁ¸ Convert*¸¦ ·¡ÇÎÇØ °¡µ¶¼º¸¸ ³ôÀÓ
    public void DestroyWallAt(Vector3Int cell)
    {
        ConvertDestructibleToBuildable(cell);
    }

    // ¿ùµå ÁÂÇ¥ ¹öÀü
    public void DestroyWallAtWorld(Vector3 worldPos)
    {
        DestroyWallAt(WorldToCell(worldPos));
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ³»ºÎ ±¸Çö
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void CacheMapsFrom(Transform stageRoot)
    {
        _grid = stageRoot.GetComponent<Grid>();
        if (_grid == null)
        {
            Debug.LogError("[MapManager] Grid not found on stage root.");
            return;
        }

        _tmBuildable = FindByName(stageRoot, "Build")?.GetComponent<Tilemap>();
        _tmUnbuildable = FindByName(stageRoot, "UnBuild")?.GetComponent<Tilemap>();
        _tmWall = FindByName(stageRoot, "UnDeWall")?.GetComponent<Tilemap>();
        _tmDestructible = FindByName(stageRoot, "DeWall")?.GetComponent<Tilemap>();
        _tmDeco = FindByName(stageRoot, "Decotile")?.GetComponent<Tilemap>();
        _tmKing = FindByName(stageRoot, "KingTile")?.GetComponent<Tilemap>();
        //_tmObjects = FindByName(stageRoot, "ObjectsTile")?.GetComponent<Tilemap>();
        _tmMonsterSpawn = FindByName(stageRoot, "MonsterSpawnTile")?.GetComponent<Tilemap>();
        _tmBossSpawn = FindByName(stageRoot, "BossSpawnTile")?.GetComponent<Tilemap>();

        Transform objRoot = FindByName(stageRoot, "ObjectsTile");
        _objectsRoot = objRoot;
        _tmObjects = objRoot ? objRoot.GetComponent<Tilemap>() : null;

        RebuildObjectsIndex();

    }

    private void RebuildObjectsIndex()
    {
        _objectCells.Clear();
        if (_objectsRoot == null || _grid == null) return;

        for (int i = 0; i < _objectsRoot.childCount; i++)
        {
            Transform child = _objectsRoot.GetChild(i);
            if (!child.gameObject.activeInHierarchy) continue;

            Vector3Int cell = _grid.WorldToCell(child.position);
            _objectCells.Add(cell);
            OnCellChanged?.Invoke(cell); 
        }
    }

    private void OnTransformChildrenChanged()
    {
        if (_objectsRoot != null) RebuildObjectsIndex();
    }

    private Transform FindByName(Transform root, string name)
    {
        if (root.name == name) return root;
        for (int i = 0; i < root.childCount; i++)
        {
            Transform child = root.GetChild(i);
            Transform found = FindByName(child, name);
            if (found) return found;
        }
        return null;
    }

    private void WireDestructibleController()
    {
        if (_tmDestructible == null) return;
        DestructibleWall ctrl = _tmDestructible.GetComponent<DestructibleWall>();
        if (!ctrl) ctrl = _tmDestructible.gameObject.AddComponent<DestructibleWall>();
        ctrl.Init(this, _tmDestructible);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¸Ê ¹Ù¿îµå/µð¹ö±×
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public BoundsInt GetNavBounds()
    {
        if (!IsReady) return new BoundsInt(Vector3Int.zero, Vector3Int.zero);

        bool any = false;
        BoundsInt mapbounds = new BoundsInt();

        void Accumulate(Tilemap tilemap)
        {
            if (!tilemap) return;
            BoundsInt bounds = tilemap.cellBounds;
            if (!any) { mapbounds = bounds; any = true; }
            else
            {
                var min = Vector3Int.Min(mapbounds.min, bounds.min);
                var max = Vector3Int.Max(mapbounds.max, bounds.max);
                mapbounds = new BoundsInt(min, max - min);
            }
        }

        Accumulate(_tmBuildable);
        Accumulate(_tmUnbuildable);
        Accumulate(_tmWall);
        Accumulate(_tmDestructible);
        Accumulate(_tmDeco);
        Accumulate(_tmKing);
        Accumulate(_tmObjects);
        Accumulate(_tmMonsterSpawn);


        return mapbounds;
    }
    // ¸Ê ÀüÃ¼ ¼¿ ¹üÀ§/Å©±â/¼¿ ¿ùµå Å©±â
    public void GetNavFrame(out Vector3Int originCell, out Vector3Int sizeCells, out Vector3 cellSize)
    {
        BoundsInt bounds = GetNavBounds();
        originCell = bounds.min;      
        sizeCells = bounds.size;      
        cellSize = _grid != null ? _grid.cellSize : Vector3.one;
    }


    // ½ÇÇè/µð¹ö±×¿ë
    public void GetCellFlags(Vector3Int cell, out bool buildable, out bool unbuildable, out bool wall, out bool destructible, out bool deco, out bool occupied, out bool objects)
    {
        buildable = _tmBuildable && _tmBuildable.HasTile(cell);
        unbuildable = _tmUnbuildable && _tmUnbuildable.HasTile(cell);
        wall = _tmWall && _tmWall.HasTile(cell);
        destructible = _tmDestructible && _tmDestructible.HasTile(cell);
        deco = _tmDeco && _tmDeco.HasTile(cell);
        occupied = _towers.ContainsKey(cell) || _occupied.Contains(cell);

        bool byTilemap = _tmObjects && _tmObjects.HasTile(cell);
        bool byChildren = _objectCells.Contains(cell);
        objects = byTilemap || byChildren;
    }

    public void DebugCheckTowerPlace(Vector3 worldPos)
    {
        if (!IsReady) return;
        var info = GetPlaceInfoWorld(worldPos);
        Debug.Log($"¼¿ {WorldToCell(worldPos)} : {(info.Placeable ? "Å¸¿ö ¼³Ä¡ °¡´É" : "¼³Ä¡ ºÒ°¡")} / Á¡À¯={info.Occupied}");
    }

}