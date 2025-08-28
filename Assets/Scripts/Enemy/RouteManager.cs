using System.Collections.Generic;
using UnityEngine;

public class RouteManager : MonoBehaviour
{
    [SerializeField] private TestMap _map;
    [SerializeField] private PathRenderer _renderer;

    [Header("Spawn Position")]
    [SerializeField] private int _spawnX = 0;
    [SerializeField] private int _spawnY = 0;
    [Header("Goal Position")]
    [SerializeField] private int _goalX = 0;
    [SerializeField] private int _goalY = 0;

    public Vector2Int SpawnCell => new Vector2Int(_spawnY, _spawnX);
    public Vector2Int GoalCell => new Vector2Int(_goalY, _goalX);

    private List<Vector2Int> _lastPath;
    private bool _allowDestructibleForRoute = false;
    public bool AllowDestructibleForRoute => _allowDestructibleForRoute;

    private void Awake()
    {
        if (!_map) _map = FindAnyObjectByType<TestMap>();
    }

    private void OnEnable()
    {
        if (_map != null) _map.OnCellChanged += HandleCellChanged;

        if (MapManager.Instance != null)
            MapManager.Instance.OnPlayerBasePlaced += HandleBasePlaced;


        if (_map && _map.TryGetSpawnCell(out Vector2Int spawnRC))
        {
            _spawnX = spawnRC.y;
            _spawnY = spawnRC.x;
        }
        SyncEndpointsFromMap();

        RebuildAndApply(force: true);
    }

    private void OnDisable()
    {
        if (_map != null) _map.OnCellChanged -= HandleCellChanged;

        if (MapManager.Instance != null)
            MapManager.Instance.OnPlayerBasePlaced -= HandleBasePlaced;
    }

    private void SyncEndpointsFromMap()
    {
        MapManager mm = MapManager.Instance;
        if (mm == null) return;

        if (mm.HasPlayerBase)
            SetGoalFromAbsCell(mm.PlayerBaseCell);
    }

    private void SetGoalFromAbsCell(Vector3Int absCell)
    {
        if (_map == null) return;
        
        Vector3 world = MapManager.Instance.GetCellCenterWorld(absCell);
        Vector2Int rc = _map.WorldToCell(world);
        _goalX = rc.y;         
        _goalY = rc.x;          
    }

    private void HandleBasePlaced(Vector3Int baseCell)
    {
        SetGoalFromAbsCell(baseCell);
        RebuildAndApply(force: true);
    }

    private void HandleCellChanged(int r, int c)
    {
        RebuildAndApply(force: false);
    }

    public void RebuildAndApply(bool force)
    {
        if (_map == null) { _renderer?.Clear(); _lastPath = null; return; }


        // 기지 설치 전에는 경로 표시 비활성

        MapManager mm = MapManager.Instance;
        if (mm == null || !mm.HasPlayerBase)
        {
            _renderer?.Clear();
            _lastPath = null;
            return;
        }

        // 기본 경로 탐색
        List<Vector2Int> path = AStarPathfinder.FindPath(
            _map.Height, _map.Width,
            (r, c) => _map.IsWalkable(r, c),
            SpawnCell, GoalCell
        );

        // 탐색 실패 시 파괴벽 허용해서 재시도
        _allowDestructibleForRoute = false;
        if (path == null || path.Count == 0)
        {
            path = AStarPathfinder.FindPath(
                _map.Height, _map.Width,
                (r, c) => _map.IsWalkable(r, c) || _map.IsDestructible(r, c),
                SpawnCell, GoalCell
            );
            _allowDestructibleForRoute = (path != null && path.Count > 0);
        }
        
        if (path == null || path.Count == 0)
        {
            _renderer?.Clear();
            _lastPath = null;
            return;
        }

        if (force || IsDifferent(_lastPath, path))
        {
            _renderer?.SetPath(_map, path);
            _lastPath = path;

            MonsterManager.Instance?.OnRouteChanged();
        }
    }


    public void SetEndpoints(Vector2Int spawn, Vector2Int goal, bool rebuildNow = true)
    {
        _spawnX = spawn.y;
        _spawnY = spawn.x;
        _goalX = goal.y;
        _goalY = goal.x;
        if (rebuildNow) RebuildAndApply(force: true);
    }

    static private bool IsDifferent(List<Vector2Int> a, List<Vector2Int> b)
    {
        if (a == null || b == null) return true;
        if (a.Count != b.Count) return true;
        for (int i = 0; i < a.Count; i++) if (a[i] != b[i]) return true;
        return false;
    }

    
}
