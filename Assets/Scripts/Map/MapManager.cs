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
    }

    public void BindStageRoot(Transform stageRoot)
    {
        UnloadStage();
        _stageRoot = stageRoot.gameObject;

        CacheMapsFrom(stageRoot);
        //SetupCollisionLayers();
        WireDestructibleController();
    }

    public void UnloadStage()
    {
        _occupied.Clear();
        _towers.Clear();
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
    // °æ·® ±¸Á¶Ã¼ & ÆíÀÇ API (¹èÄ¡/±æÃ£±â Àü¿ë)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    public readonly struct PlaceInfo
    {
        public readonly Vector3Int cell;
        public readonly bool placeable; // Áö±Ý Áï½Ã ¼³Ä¡ °¡´ÉÇÑ°¡?
        public readonly bool occupied;  // Á¡À¯µÅ ÀÖ´Â°¡?

        public PlaceInfo(Vector3Int cell, bool placeable, bool occupied)
        { this.cell = cell; this.placeable = placeable; this.occupied = occupied; }
    }
    public PlaceInfo GetPlaceInfo(Vector3Int cell)
    {
        bool occupied = _towers.ContainsKey(cell) || _occupied.Contains(cell);
        bool placeable =
            (_tmBuildable && _tmBuildable.HasTile(cell)) &&
            !occupied &&
            !((_tmWall && _tmWall.HasTile(cell)) || (_tmDestructible && _tmDestructible.HasTile(cell)));

        return new PlaceInfo(cell, placeable, occupied);
    }
    public PlaceInfo GetPlaceInfoWorld(Vector3 worldPos) => GetPlaceInfo(WorldToCell(worldPos));

    public readonly struct NavInfo
    {
        public readonly Vector3Int cell;
        public readonly bool blocked;        // ÀÌµ¿ ºÒ°¡ ÀüÃ¼ ÆÇ´Ü
        public readonly bool blockedByTower; // Å¸¿ö/Á¡À¯·Î ÀÎÇØ ¸·Èû
        public readonly bool blockedByWall;  // º®/ÆÄ±«º®À¸·Î ¸·Èû

        public NavInfo(Vector3Int cell, bool blocked, bool blockedByTower, bool blockedByWall)
        { this.cell = cell; this.blocked = blocked; this.blockedByTower = blockedByTower; this.blockedByWall = blockedByWall; }
    }
    public NavInfo GetNavInfo(Vector3Int cell)
    {
        bool byWall = (_tmWall && _tmWall.HasTile(cell)) || (_tmDestructible && _tmDestructible.HasTile(cell));
        bool byTower = _towers.ContainsKey(cell) || _occupied.Contains(cell);
        bool blocked = byWall || byTower;
        return new NavInfo(cell, blocked, byTower, byWall);
    }
    public NavInfo GetNavInfoWorld(Vector3 worldPos) => GetNavInfo(WorldToCell(worldPos));

    //
    //public bool IsTowerPlaceableCell(Vector3Int cell) => GetPlaceInfo(cell).placeable;
    //public bool IsTowerPlaceableWorld(Vector3 worldPos) => GetPlaceInfoWorld(worldPos).placeable;
    //
    //public bool IsBlockedCell(Vector3Int cell) => GetNavInfo(cell).blocked;
    //public bool IsBlockedWorld(Vector3 worldPos) => GetNavInfoWorld(worldPos).blocked;
    //
    //public bool IsBlockedByTower(Vector3Int cell) => GetNavInfo(cell).blockedByTower;

    //¸ó½ºÅÍ°¡ Å¸¿ö °ø°Ý ÇÏ´Â ¿ë
    public bool TryGetTowerAt(Vector3Int cell, out GameObject tower) => _towers.TryGetValue(cell, out tower);
    //
    //public bool HasTower(Vector3Int cell) => _towers.ContainsKey(cell);
    //public bool IsWall(Vector3Int cell) => _tmWall && _tmWall.HasTile(cell);
    //public bool IsDestructible(Vector3Int cell) => _tmDestructible && _tmDestructible.HasTile(cell);
    //public bool IsBuildable(Vector3Int cell) => _tmBuildable && _tmBuildable.HasTile(cell);

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

    //private void SetupCollisionLayers()
    //{
    //    // Buildable: Ãæµ¹ X
    //    DisableCollider(_tmBuildable);
    //
    //    // Wall/Destructible: Ãæµ¹ O
    //    SetupCollider(_tmWall, CompositeCollider2D.GeometryType.Outlines);
    //    SetupCollider(_tmDestructible, CompositeCollider2D.GeometryType.Outlines);
    //
    //    // Deco, UnBuildable: Ãæµ¹ X (ÇÊ¿ä ½Ã UnBuildable¿¡ Ãæµ¹ ÄÑµµ µÊ)
    //    DisableCollider(_tmDeco);
    //    DisableCollider(_tmUnbuildable);
    //}
    //
    //private void DisableCollider(Tilemap tilemap)
    //{
    //    if (!tilemap) return;
    //    TilemapCollider2D collider = tilemap.GetComponent<TilemapCollider2D>();
    //    if (collider) collider.enabled = false;
    //    Rigidbody2D rigid = tilemap.GetComponent<Rigidbody2D>();
    //    if (rigid) rigid.simulated = false;
    //    CompositeCollider2D composite = tilemap.GetComponent<CompositeCollider2D>();
    //    if (composite) composite.enabled = false;
    //}
    //
    //private void SetupCollider(Tilemap tilemap, CompositeCollider2D.GeometryType geoType)
    //{
    //    if (!tilemap) return;
    //
    //    TilemapCollider2D tileCol = tilemap.GetComponent<TilemapCollider2D>() ?? tilemap.gameObject.AddComponent<TilemapCollider2D>();
    //    tileCol.isTrigger = false;
    //    tileCol.usedByComposite = true;
    //
    //    CompositeCollider2D composite = tilemap.GetComponent<CompositeCollider2D>() ?? tilemap.gameObject.AddComponent<CompositeCollider2D>();
    //    composite.geometryType = geoType;
    //    composite.generationType = CompositeCollider2D.GenerationType.Synchronous;
    //
    //    Rigidbody2D rigid = tilemap.GetComponent<Rigidbody2D>() ?? tilemap.gameObject.AddComponent<Rigidbody2D>();
    //    rigid.bodyType = RigidbodyType2D.Static;
    //}
    //
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

        return mapbounds;
    }
    // ¸Ê ÀüÃ¼ ¼¿ ¹üÀ§/Å©±â/¼¿ ¿ùµå Å©±â
    public void GetNavFrame(out Vector3Int originCell, out Vector3Int sizeCells, out Vector3 cellSize)
    {
        BoundsInt b = GetNavBounds();
        originCell = b.min;      
        sizeCells = b.size;      
        cellSize = _grid != null ? _grid.cellSize : Vector3.one;
    }


    // ½ÇÇè/µð¹ö±×¿ë
    public void GetCellFlags(Vector3Int c, out bool buildable, out bool unbuildable, out bool wall, out bool destructible, out bool deco, out bool occupied)
    {
        buildable = _tmBuildable && _tmBuildable.HasTile(c);
        unbuildable = _tmUnbuildable && _tmUnbuildable.HasTile(c);
        wall = _tmWall && _tmWall.HasTile(c);
        destructible = _tmDestructible && _tmDestructible.HasTile(c);
        deco = _tmDeco && _tmDeco.HasTile(c);
        occupied = _towers.ContainsKey(c) || _occupied.Contains(c);
    }

    public void DebugCheckTowerPlace(Vector3 worldPos)
    {
        if (!IsReady) return;
        var info = GetPlaceInfoWorld(worldPos);
        Debug.Log($"¼¿ {WorldToCell(worldPos)} : {(info.placeable ? "Å¸¿ö ¼³Ä¡ °¡´É" : "¼³Ä¡ ºÒ°¡")} / Á¡À¯={info.occupied}");
    }
}