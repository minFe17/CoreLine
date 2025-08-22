using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance => Utils.MonoSingleton<MapManager>.Instance;
    private bool _hasSpawn;
    private Vector3Int _spawnCell;
    private GameObject _stageRoot;
    private Grid _grid;
    private Tilemap _tmBuildable, _tmUnbuildable, _tmWall, _tmDestructible, _tmDeco, _tmKing, _tmObjects, _tmMonsterSpawn;

    private readonly HashSet<Vector3Int> _occupied = new();
    private readonly Dictionary<Vector3Int, GameObject> _towers = new(); // �� �� Ÿ�� ������Ʈ
    public bool IsReady => _grid != null;

    // �׺�/��ġ ���� �˸�(Ÿ�� ��ġ/����, �ı��� ���� ��)
    public Action<Vector3Int> OnCellChanged;

    // ����������������������������������������������������������������������������������������������������������������������������������������������
    // �������� �ε�/���ε�/��ε�
    // ����������������������������������������������������������������������������������������������������������������������������������������������
    private void Update()
    {
        if (Input.GetMouseButtonDown(2)) // ��Ŭ�� �׽�Ʈ
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f; // 2D�ϱ� z ����
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
    }

    public void BindStageRoot(Transform stageRoot)
    {
        UnloadStage();
        _stageRoot = stageRoot.gameObject;

        CacheMapsFrom(stageRoot);
        //SetupCollisionLayers();
        WireDestructibleController();
        InitSpawnCell();
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
                    Debug.LogWarning($"[MapManager] Spawn Ÿ���� ���� ���Դϴ�. ù ��°({_spawnCell})�� ���, ������ {cell} ����.");
                    continue;
                }
                _spawnCell = cell;
                _hasSpawn = true;
            }
        }
    }

    // ����������������������������������������������������������������������������������������������������������������������������������������������
    // ��ǥ ��ƿ
    // ����������������������������������������������������������������������������������������������������������������������������������������������
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

    // ����������������������������������������������������������������������������������������������������������������������������������������������
    // �淮 ����ü & ���� API (��ġ/��ã�� ����)
    // ����������������������������������������������������������������������������������������������������������������������������������������������

    public readonly struct PlaceInfo
    {
        public readonly Vector3Int Cell;
        public readonly bool Placeable; // ���� ��� ��ġ �����Ѱ�?
        public readonly bool Occupied;  // ������ �ִ°�?

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

        return new PlaceInfo(cell, placeable, occupied);
    }
    public PlaceInfo GetPlaceInfoWorld(Vector3 worldPos) => GetPlaceInfo(WorldToCell(worldPos));
    // �������� ���� �� �÷��̾� ���̽� ��ġ ���� Ÿ�� Get��
    // ���� ���� ��� KingTile �� ����
    public List<Vector3Int> GetAllKingCells()
    {
        var list = new List<Vector3Int>();
        if (_tmKing == null) return list;
        foreach (var c in _tmKing.cellBounds.allPositionsWithin)
            if (_tmKing.HasTile(c)) list.Add(c);
        return list;
    }

    // ���õ� KingTile�� Buildable�� ��ȯ
    public bool ConvertKingToBuildable(Vector3Int cell)
    {
        if (_tmKing == null || !_tmKing.HasTile(cell)) return false;
        _tmKing.SetTile(cell, null);
        OnCellChanged?.Invoke(cell);  // ��ã��/��ġ ���� �˸�
        return true;
    }
    public bool SelectPlayerBase(Vector3Int selectedCell, GameObject basePrefab = null, bool occupyBaseCell = true)
    {
        if (_tmKing == null) return false;
        if (!_tmKing.HasTile(selectedCell)) return false; // King �ĺ��� �ƴ� ĭ�̸� ����

        // 1) King �ĺ��� ����
        var kings = GetAllKingCells();
        if (kings.Count == 0) return false;

        // 2) ��� ó��
        foreach (var c in kings)
        {
            // King Ÿ�� ����
            _tmKing.SetTile(c, null);

            if (c == selectedCell)
            {
                // ���� ĭ: ���̽� Ȯ��
                if (basePrefab != null)
                {
                    var pos = CellCenterWorld(c);
                    var go = Instantiate(basePrefab, pos, Quaternion.identity, _stageRoot?.transform);
                    go.name = basePrefab.name;
                }

                if (occupyBaseCell)
                    MarkOccupied(c); // ���̽� ���� Ÿ�� ��ġ �Ұ��� ����
            }
            OnCellChanged?.Invoke(c);
        }
        return true;
    }

    public readonly struct NavInfo
    {
        public readonly Vector3Int Cell;
        public readonly bool Blocked;        // �̵� �Ұ� ��ü �Ǵ�
        public readonly bool BlockedByTower; // Ÿ��/������ ���� ����
        public readonly bool BlockedByWall;  // ��/�ı������� ����

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
    //���� ���� Ÿ�� Get��
    //public List<Vector3Int> GetSpawnCells()
    //{
    //    var list = new List<Vector3Int>();
    //    if (_tmMonsterSpawn == null) return list;
    //    foreach (var c in _tmMonsterSpawn.cellBounds.allPositionsWithin)
    //        if (_tmMonsterSpawn.HasTile(c)) list.Add(c);
    //    return list;
    //}
    //�Ǽ� ����
    public bool TryGetSpawnCell(out Vector3Int cell)
    {
        cell = _spawnCell;
        return _hasSpawn;
    }
    //���� ���� Ÿ�� Get��
    public Vector3 GetSpawnWorld()
    {
        return _hasSpawn ? CellCenterWorld(_spawnCell) : Vector3.zero;
    }
    //
    //public bool IsTowerPlaceableCell(Vector3Int cell) => GetPlaceInfo(cell).placeable;
    //public bool IsTowerPlaceableWorld(Vector3 worldPos) => GetPlaceInfoWorld(worldPos).placeable;
    //
    //public bool IsBlockedCell(Vector3Int cell) => GetNavInfo(cell).blocked;
    //public bool IsBlockedWorld(Vector3 worldPos) => GetNavInfoWorld(worldPos).blocked;
    //
    //public bool IsBlockedByTower(Vector3Int cell) => GetNavInfo(cell).blockedByTower;

    //���Ͱ� Ÿ�� ���� �ϴ� ��
    public bool TryGetTowerAt(Vector3Int cell, out GameObject tower) => _towers.TryGetValue(cell, out tower);
    //
    //public bool HasTower(Vector3Int cell) => _towers.ContainsKey(cell);
    //public bool IsWall(Vector3Int cell) => _tmWall && _tmWall.HasTile(cell);
    //public bool IsDestructible(Vector3Int cell) => _tmDestructible && _tmDestructible.HasTile(cell);
    //public bool IsBuildable(Vector3Int cell) => _tmBuildable && _tmBuildable.HasTile(cell);

    // ����������������������������������������������������������������������������������������������������������������������������������������������
    // ��ġ/���� ����
    // ����������������������������������������������������������������������������������������������������������������������������������������������
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

    // ��ġ ���� ȣ��: RegisterTower(cell, towerInstance);
    public void RegisterTower(Vector3Int cell, GameObject tower)
    {
        _towers[cell] = tower;
        _occupied.Add(cell);              // ���� ǥ��
        OnCellChanged?.Invoke(cell);
    }

    // �ı�/�Ǹ� �� ȣ��: UnregisterTower(cell);
    public void UnregisterTower(Vector3Int cell)
    {
        _towers.Remove(cell);
        _occupied.Remove(cell);
        OnCellChanged?.Invoke(cell);
    }

    // ����������������������������������������������������������������������������������������������������������������������������������������������
    // �ı��� ó��
    // ����������������������������������������������������������������������������������������������������������������������������������������������
    public void ConvertDestructibleToBuildable(Vector3Int cell)
    {
        if (!IsReady || _tmDestructible == null) return;
        if (_tmDestructible.HasTile(cell))
        {
            _tmDestructible.SetTile(cell, null); // ���� Buildable�� �״�� �巯��
            //_tmDestructible.GetComponent<TilemapCollider2D>()?.ProcessTilemapChanges();
            OnCellChanged?.Invoke(cell);
        }
    }

    // ����������������������������������������������������������������������������������������������������������������������������������������������
    // ���� ����
    // ����������������������������������������������������������������������������������������������������������������������������������������������
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
        _tmObjects = FindByName(stageRoot, "ObjectsTile")?.GetComponent<Tilemap>();
        _tmMonsterSpawn = FindByName(stageRoot, "MonsterSpawnTile")?.GetComponent<Tilemap>();
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
    //    // Buildable: �浹 X
    //    DisableCollider(_tmBuildable);
    //
    //    // Wall/Destructible: �浹 O
    //    SetupCollider(_tmWall, CompositeCollider2D.GeometryType.Outlines);
    //    SetupCollider(_tmDestructible, CompositeCollider2D.GeometryType.Outlines);
    //
    //    // Deco, UnBuildable: �浹 X (�ʿ� �� UnBuildable�� �浹 �ѵ� ��)
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

    // ����������������������������������������������������������������������������������������������������������������������������������������������
    // ������Ʈ Ÿ�� ��ȣ�ۿ� ����
    // ����������������������������������������������������������������������������������������������������������������������������������������������
    // �ش� ���� ObjectsTile�ΰ�?
    //public bool IsObjectTile(Vector3Int cell) => _tmObjects && _tmObjects.HasTile(cell);

    // ȿ�� Ʈ���� �̺�Ʈ(Ÿ�� �̸��� �Բ� �ѱ�)
    //public event Action<Vector3Int, string> OnObjectTileTriggered;

    // ȿ�� ���: Ÿ�� ���� + �̺�Ʈ ����
    //public bool UseObjectTile(Vector3Int cell)
    //{
    //    if (_tmObjects == null || !_tmObjects.HasTile(cell)) return false;
    //
    //    // � ������Ʈ������ �ĺ�(Ÿ�� �̸� Ȱ��)
    //    string tileName = _tmObjects.GetTile(cell)?.name ?? "Object";
    //    _tmObjects.SetTile(cell, null);
    //
    //    OnObjectTileTriggered?.Invoke(cell, tileName);
    //    OnCellChanged?.Invoke(cell); // �ʿ� �� �׺� ����(�������̶� ������ ����)
    //    return true;
    //}

    // ����������������������������������������������������������������������������������������������������������������������������������������������
    // �� �ٿ��/�����
    // ����������������������������������������������������������������������������������������������������������������������������������������������
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
    // �� ��ü �� ����/ũ��/�� ���� ũ��
    public void GetNavFrame(out Vector3Int originCell, out Vector3Int sizeCells, out Vector3 cellSize)
    {
        BoundsInt b = GetNavBounds();
        originCell = b.min;      
        sizeCells = b.size;      
        cellSize = _grid != null ? _grid.cellSize : Vector3.one;
    }


    // ����/����׿�
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
        Debug.Log($"�� {WorldToCell(worldPos)} : {(info.Placeable ? "Ÿ�� ��ġ ����" : "��ġ �Ұ�")} / ����={info.Occupied}");
    }
}