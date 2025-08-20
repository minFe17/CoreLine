using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance => Utils.MonoSingleton<MapManager>.Instance;

    private GameObject _stageRoot;
    private Grid _grid;
    private Tilemap _tmBuildable, _tmUnbuildable, _tmWall, _tmDestructible, _tmDeco;

    private readonly HashSet<Vector3Int> _occupied = new();
    private readonly Dictionary<Vector3Int, GameObject> _towers = new(); // ¼¿ ¡æ Å¸¿ö ¿ÀºêÁ§Æ®
    public bool IsReady => _grid != null;

    // ³×ºñ/¹èÄ¡ º¯°æ ¾Ë¸²(Å¸¿ö ¼³Ä¡/Á¦°Å, ÆÄ±«º® º¯°æ µî)
    public Action<Vector3Int> OnCellChanged;

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ½ºÅ×ÀÌÁö ·Îµå/¹ÙÀÎµå/¾ð·Îµå
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void Update()
    {
        if (Input.GetMouseButtonDown(2)) // ÁÂÅ¬¸¯
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f; // 2D´Ï±î z °íÁ¤
            MapManager.Instance.DebugCheckTowerPlace(mouseWorld);
        }
    }
    public void LoadStage(GameObject stagePrefab)
    {
        UnloadStage();
        _stageRoot = Instantiate(stagePrefab);
        _stageRoot.name = stagePrefab.name;

        CacheMapsFrom(_stageRoot.transform);
        SetupCollisionLayers();
        WireDestructibleController();
    }

    public void BindStageRoot(Transform stageRoot)
    {
        UnloadStage();
        _stageRoot = stageRoot.gameObject;

        CacheMapsFrom(stageRoot);
        SetupCollisionLayers();
        WireDestructibleController();
    }

    public void UnloadStage()
    {
        _occupied.Clear();
        _grid = null;
        _tmBuildable = _tmUnbuildable = _tmWall = _tmDestructible = _tmDeco = null;

        if (_stageRoot != null)
        {
            Destroy(_stageRoot);
            _stageRoot = null;
        }
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

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¹èÄ¡ °¡´É ¿©ºÎ & Á¡À¯
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public bool IsBuildableCell(Vector3Int cell)
    {
        if (!IsReady) return false;
        if (_tmBuildable == null || !_tmBuildable.HasTile(cell)) return false;   // ºôµå°¡´É Å¸ÀÏ ·¹ÀÌ¾î¿¡ Å¸ÀÏÀÌ ÀÖ¾î¾ß ÇÔ
        if (_occupied.Contains(cell)) return false;                               // ÀÌ¹Ì Å¸¿ö µîÀ¸·Î Á¡À¯µÊ
        if ((_tmWall && _tmWall.HasTile(cell)) || (_tmDestructible && _tmDestructible.HasTile(cell))) return false; // º®/ÆÄ±«º®ÀÌ¸é ºÒ°¡
        return true;
    }

    public bool IsTowerPlaceableCell(Vector3Int cell) => IsBuildableCell(cell); // º°Äª

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

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÆÄ±«º® ¡æ ¹Ø¿¡ ±ò·ÁÀÖ´ø Buildable ³ëÃâ
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void ConvertDestructibleToBuildable(Vector3Int cell)
    {
        if (!IsReady || _tmDestructible == null) return;
        if (_tmDestructible.HasTile(cell))
        {
            _tmDestructible.SetTile(cell, null); // ¹ØÀÇ BuildableÀÌ ±×´ë·Î µå·¯³²
            _tmDestructible.GetComponent<TilemapCollider2D>()?.ProcessTilemapChanges();
            OnCellChanged?.Invoke(cell);
        }
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

        _tmBuildable = FindByName(stageRoot, "Buildable")?.GetComponent<Tilemap>();
        _tmUnbuildable = FindByName(stageRoot, "UnBuildable")?.GetComponent<Tilemap>();
        _tmWall = FindByName(stageRoot, "Wall")?.GetComponent<Tilemap>();
        _tmDestructible = FindByName(stageRoot, "DestructibleWall")?.GetComponent<Tilemap>();
        _tmDeco = FindByName(stageRoot, "Deco")?.GetComponent<Tilemap>();
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

    private void SetupCollisionLayers()
    {
        // Buildable: Ãæµ¹ X
        DisableCollider(_tmBuildable);

        // Wall/Destructible: Ãæµ¹ O
        SetupCollider(_tmWall, CompositeCollider2D.GeometryType.Outlines);
        SetupCollider(_tmDestructible, CompositeCollider2D.GeometryType.Outlines);

        // Deco, UnBuildable: Ãæµ¹ X (ÇÊ¿ä ½Ã UnBuildable¿¡ Ãæµ¹ ÄÑµµ µÊ)
        DisableCollider(_tmDeco);
        DisableCollider(_tmUnbuildable);
    }

    private void DisableCollider(Tilemap tilemap)
    {
        if (!tilemap) return;
        TilemapCollider2D collider = tilemap.GetComponent<TilemapCollider2D>();
        if (collider) collider.enabled = false;
        Rigidbody2D rigid = tilemap.GetComponent<Rigidbody2D>();
        if (rigid) rigid.simulated = false;
        CompositeCollider2D compsite = tilemap.GetComponent<CompositeCollider2D>();
        if (compsite) compsite.enabled = false;
    }

    private void SetupCollider(Tilemap tilemap, CompositeCollider2D.GeometryType geoType)
    {
        if (!tilemap) return;

        TilemapCollider2D tileCol = tilemap.GetComponent<TilemapCollider2D>() ?? tilemap.gameObject.AddComponent<TilemapCollider2D>();
        tileCol.isTrigger = false;
        tileCol.usedByComposite = true;

        CompositeCollider2D compsite = tilemap.GetComponent<CompositeCollider2D>() ?? tilemap.gameObject.AddComponent<CompositeCollider2D>();
        compsite.geometryType = geoType;
        compsite.generationType = CompositeCollider2D.GenerationType.Synchronous;

        Rigidbody2D rigid = tilemap.GetComponent<Rigidbody2D>() ?? tilemap.gameObject.AddComponent<Rigidbody2D>();
        rigid.bodyType = RigidbodyType2D.Static;
    }

    private void WireDestructibleController()
    {
        if (_tmDestructible == null) return;
        DestructibleWall ctrl = _tmDestructible.GetComponent<DestructibleWall>();
        if (!ctrl) ctrl = _tmDestructible.gameObject.AddComponent<DestructibleWall>();
        ctrl.Init(this, _tmDestructible);
    }
    //½ÇÇè¿ë
    public void GetCellFlags( Vector3Int c, out bool buildable, out bool unbuildable, out bool wall, out bool destructible, out bool deco, out bool occupied) 
    { 
        buildable = _tmBuildable && _tmBuildable.HasTile(c); 
        unbuildable = _tmUnbuildable && _tmUnbuildable.HasTile(c);
        wall = _tmWall && _tmWall.HasTile(c);
        destructible = _tmDestructible && _tmDestructible.HasTile(c);
        deco = _tmDeco && _tmDeco.HasTile(c); occupied = _occupied.Contains(c);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ±æÃ£±â/¸ó½ºÅÍ Àü¿ë Å¸¿ö ¹èÄ¡ °¡´É ¿©ºÎ Æ÷ÇÔ
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public struct CellInfo
    {
        public bool buildable;        // Buildable Å¸ÀÏ ·¹ÀÌ¾î¿¡ Å¸ÀÏ Á¸Àç
        public bool unbuildable;      // UnBuildable Å¸ÀÏ Á¸Àç
        public bool wall;             // Wall Å¸ÀÏ Á¸Àç
        public bool destructible;     // DestructibleWall Å¸ÀÏ Á¸Àç(ÆÄ±« Àü true)
        public bool deco;             // Deco Å¸ÀÏ Á¸Àç
        public bool occupied;         // Å¸¿ö µîÀ¸·Î Á¡À¯µÊ
        public bool blocked;          // ÀÌµ¿ ºÒ°¡(= wall || destructible || occupied)
        public bool towerPlaceable;   // Áö±Ý Áï½Ã Å¸¿ö ¹èÄ¡ °¡´É? (IsBuildableCell °á°ú)
    }

    public CellInfo GetCellInfo(Vector3Int cell)
    {
        CellInfo info = new CellInfo
        {
            buildable = _tmBuildable && _tmBuildable.HasTile(cell),
            unbuildable = _tmUnbuildable && _tmUnbuildable.HasTile(cell),
            wall = _tmWall && _tmWall.HasTile(cell),
            destructible = _tmDestructible && _tmDestructible.HasTile(cell),
            deco = _tmDeco && _tmDeco.HasTile(cell),
            occupied = _towers.ContainsKey(cell) || _occupied.Contains(cell), // ¡ç Å¸¿ö ±âÁØ ¿ì¼±
        };
        info.blocked = info.wall || info.destructible || info.occupied;
        info.towerPlaceable = IsBuildableCell(cell);
        return info;
    }

    // ¼³Ä¡ Á÷ÈÄ È£Ãâ: RegisterTower(cell, towerInstance);
    public void RegisterTower(Vector3Int cell, GameObject tower)
    {
        _towers[cell] = tower;
        _occupied.Add(cell);              // ±âÁ¸ ·ÎÁ÷°ú È£È¯(Á¡À¯ Ç¥±â)
        OnCellChanged?.Invoke(cell);
    }

    // ÆÄ±«/ÆÇ¸Å ½Ã È£Ãâ: UnregisterTower(cell);
    public void UnregisterTower(Vector3Int cell)
    {
        _towers.Remove(cell);
        _occupied.Remove(cell);
        OnCellChanged?.Invoke(cell);
    }
    public bool IsBlockedCell(Vector3Int cell)
    {
        if (!IsReady) return true;
        if ((_tmWall && _tmWall.HasTile(cell)) || (_tmDestructible && _tmDestructible.HasTile(cell)))
            return true;
        // ¡å Å¸¿ö°¡ ÀÖÀ¸¸é ¸·Èû
        if (_towers.ContainsKey(cell)) return true;
        // (±âÁ¸ _occupiedµµ ³²°ÜµÎ°í ½Í´Ù¸é)
        return _occupied.Contains(cell);
    }

    public bool IsBlockedByTower(Vector3Int cell) => _towers.ContainsKey(cell);
    public bool TryGetTowerAt(Vector3Int cell, out GameObject tower) => _towers.TryGetValue(cell, out tower);

    // ±âÁ¸ HasTower´Â µñ¼Å³Ê¸® ±âÁØÀ¸·Î ¹Ù²ãÄ¡±â
    public bool HasTower(Vector3Int cell) => _towers.ContainsKey(cell);
    public bool IsWall(Vector3Int cell) => _tmWall && _tmWall.HasTile(cell);
    public bool IsDestructible(Vector3Int cell) => _tmDestructible && _tmDestructible.HasTile(cell);
    public bool IsBuildable(Vector3Int cell) => _tmBuildable && _tmBuildable.HasTile(cell);

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

        return mapbounds;
    }
    //µð¹ö±×¿ë
    public void DebugCheckTowerPlace(Vector3 worldPos)
    {
        if (!IsReady) return;

        Vector3Int cell = WorldToCell(worldPos);
        if (IsBuildableCell(cell))
        {
            Debug.Log($"¼¿ {cell} : Å¸¿ö ¼³Ä¡ °¡´É");
        }
        else
        {
            Debug.Log($"¼¿ {cell} : ¼³Ä¡ ºÒ°¡");
        }
    }
}
