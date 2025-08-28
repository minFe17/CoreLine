using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TestMap _map;
    [SerializeField] private RouteManager _route;
    [SerializeField] private MonsterMover _monsterPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int _initialSpawnCount = 5;
    [SerializeField] private bool _snapToCellCenter = true;
    [SerializeField] private bool _spawnOnStart = false;

    private readonly List<MonsterMover> _monsters = new List<MonsterMover>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        if (MapManager.Instance != null)
            MapManager.Instance.OnPlayerBasePlaced += HandleBasePlaced;
    }

    private void OnDisable()
    {
        if (MapManager.Instance != null)
            MapManager.Instance.OnPlayerBasePlaced -= HandleBasePlaced;
    }

    private void Start()
    {
        if (!_map) _map = FindAnyObjectByType<TestMap>();
        if (!_route) _route = FindAnyObjectByType<RouteManager>();

        if (_spawnOnStart && MapManager.Instance != null && MapManager.Instance.HasPlayerBase)
        {
            EnsureRouteReady();
            SpawnWave(_initialSpawnCount, sendToGoalImmediately: true);
        }
    }

   
    private void HandleBasePlaced(Vector3Int baseCell)
    {
        EnsureRouteReady();                        
        SpawnWave(_initialSpawnCount, sendToGoalImmediately: false);       
    }

    private void EnsureRouteReady()
    {
        if (_route != null)
            _route.RebuildAndApply(force: true);    
    }

   
    public MonsterMover SpawnOne()
    {
        if (!_monsterPrefab || !_map || !_route) return null;
        if (MapManager.Instance != null && !MapManager.Instance.HasPlayerBase) return null; 

        Vector2Int spawnRC = _route.SpawnCell;
        Vector3 pos = _map.CellToWorld(spawnRC.x, spawnRC.y);
        if (_snapToCellCenter) pos.z = 0f;

        MonsterMover m = Instantiate(_monsterPrefab, pos, Quaternion.identity);
        m.Map = _map;
        _monsters.Add(m);
        return m;
    }

    public void SpawnWave(int n, bool sendToGoalImmediately = false)
    {
        if (n <= 0) return;

        for (int i = 0; i < n; i++)
            SpawnOne();

        if (sendToGoalImmediately)
            SendAllToGoal();
    }

    public void SendAllToGoal()
    {
        if (!_route) return;

        var allowance = _route.Allowance; // RouteAllowance

        foreach (MonsterMover m in _monsters)
        {
            if (!m) continue;
            bool allowWalls = (allowance == RouteManager.RouteAllowance.WallsOnly || allowance == RouteManager.RouteAllowance.WallsAndTowers);
            bool allowTowers = (allowance == RouteManager.RouteAllowance.WallsAndTowers);

            m.MoveToCell(_route.GoalCell, allowWalls, allowTowers);
        }
    }

    
    public void OnRouteChanged()
    {
        SendAllToGoal();
    }
}
