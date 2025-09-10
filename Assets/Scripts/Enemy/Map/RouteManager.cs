using System.Collections.Generic;
using UnityEngine;

public sealed class RouteManager : MonoBehaviour
{
    private const string EVT_STAGE_LOADED = "EVT_STAGE_LOADED";

    [SerializeField] private TestMap _map;
    [SerializeField] private PathRenderer _renderer;

    [Header("Spawn Position")]
    [SerializeField] private int _spawnX = 0;
    [SerializeField] private int _spawnY = 0;

    [Header("Goal Position")]
    [SerializeField] private int _goalX = 0;
    [SerializeField] private int _goalY = 0;

    public Vector2Int SpawnCell { get { return new Vector2Int(_spawnY, _spawnX); } }
    public Vector2Int GoalCell { get { return new Vector2Int(_goalY, _goalX); } }

    private List<Vector2Int> _lastPath;
    private bool _allowDestructibleForRoute = false;
    public bool AllowDestructibleForRoute { get { return _allowDestructibleForRoute; } }

    public enum RouteAllowance { None, WallsOnly, WallsAndTowers }
    private RouteAllowance _routeAllowance = RouteAllowance.None;
    public RouteAllowance Allowance { get { return _routeAllowance; } }

    private void Awake()
    {
        if (_map == null) { _map = FindAnyObjectByType<TestMap>(); }
    }

    private void OnEnable()
    {
        if (_map != null) { _map.OnCellChanged += HandleCellChanged; }

        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnPlayerBasePlaced += HandleBasePlaced;
        }

        EventManager.Instance.Subscribe<string>(GameManager.EVT_STAGE_LOADED, OnStageLoaded);

        // 이미 로드되어 준비된 경우 즉시 1회 반영
        if (MapManager.Instance != null && MapManager.Instance.IsReady)
        {
            SyncEndpointsFromMap();
            RebuildAndApply(true);
        }
    }

    private void OnDisable()
    {
        if (_map != null) { _map.OnCellChanged -= HandleCellChanged; }

        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnPlayerBasePlaced -= HandleBasePlaced;
        }

        EventManager.Instance.UnSubscribe(GameManager.EVT_STAGE_LOADED, (System.Action<string>)OnStageLoaded);
    }

    private void OnStageLoaded(string stageId)
    {
        SyncEndpointsFromMap();
        RebuildAndApply(true);
    }

    private void SyncEndpointsFromMap()
    {
        MapManager mm = MapManager.Instance;
        if (mm == null) { return; }

        if (_map != null && _map.TryGetSpawnCell(out Vector2Int spawnRC))
        {
            _spawnX = spawnRC.y;
            _spawnY = spawnRC.x;
        }

        if (mm.HasPlayerBase)
        {
            SetGoalFromAbsCell(mm.PlayerBaseCell);
        }
    }

    private void SetGoalFromAbsCell(Vector3Int absCell)
    {
        if (_map == null) { return; }

        Vector3 world = MapManager.Instance.GetCellCenterWorld(absCell);
        Vector2Int rc = _map.WorldToCell(world);
        _goalX = rc.y;
        _goalY = rc.x;
    }

    private void HandleBasePlaced(Vector3Int baseCell)
    {
        SetGoalFromAbsCell(baseCell);
        RebuildAndApply(true);
    }

    private void HandleCellChanged(int r, int c)
    {
        RebuildAndApply(false);
    }

    public void RebuildAndApply(bool force)
    {
        if (_map == null)
        {
            if (_renderer != null) { _renderer.Clear(); }
            _lastPath = null;
            _routeAllowance = RouteAllowance.None;
            return;
        }

        MapManager mm = MapManager.Instance;
        if (mm == null || !mm.HasPlayerBase)
        {
            if (_renderer != null) { _renderer.Clear(); }
            _lastPath = null;
            _routeAllowance = RouteAllowance.None;
            return;
        }

        List<Vector2Int> path = AStarPathfinder.FindPath(
            _map.Height, _map.Width,
            (int r, int c) => _map.IsWalkable(r, c),
            SpawnCell, GoalCell
        );
        _routeAllowance = RouteAllowance.None;

        if (path == null || path.Count == 0)
        {
            path = AStarPathfinder.FindPath(
                _map.Height, _map.Width,
                (int r, int c) => _map.IsWalkable(r, c) || _map.IsDestructible(r, c),
                SpawnCell, GoalCell
            );
            if (path != null && path.Count > 0) { _routeAllowance = RouteAllowance.WallsOnly; }
        }

        if (path == null || path.Count == 0)
        {
            path = AStarPathfinder.FindPath(
                _map.Height, _map.Width,
                (int r, int c) => _map.IsWalkable(r, c) || _map.IsDestructible(r, c) || _map.HasTower(r, c),
                SpawnCell, GoalCell
            );
            if (path != null && path.Count > 0) { _routeAllowance = RouteAllowance.WallsAndTowers; }
        }

        if (path == null || path.Count == 0)
        {
            if (_renderer != null) { _renderer.Clear(); }
            _lastPath = null;
            _routeAllowance = RouteAllowance.None;
            return;
        }

        if (force || IsDifferent(_lastPath, path))
        {
            if (_renderer != null) { _renderer.SetPath(_map, path); }
            _lastPath = path;
            if (MonsterManager.Instance != null) { MonsterManager.Instance.OnRouteChanged(); }
        }
    }

    public void SetEndpoints(Vector2Int spawn, Vector2Int goal, bool rebuildNow = true)
    {
        _spawnX = spawn.y;
        _spawnY = spawn.x;
        _goalX = goal.y;
        _goalY = goal.x;
        if (rebuildNow) { RebuildAndApply(true); }
    }

    private static bool IsDifferent(List<Vector2Int> a, List<Vector2Int> b)
    {
        if (a == null || b == null) { return true; }
        if (a.Count != b.Count) { return true; }
        for (int i = 0; i < a.Count; i++) { if (a[i] != b[i]) { return true; } }
        return false;
    }
}
