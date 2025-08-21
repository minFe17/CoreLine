using System.Collections;
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
    [SerializeField] private bool _spawnOnStart = true;

    private readonly List<MonsterMover> _monsters = new List<MonsterMover>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (!_map) _map = FindAnyObjectByType<TestMap>();
        if (!_route) _route = FindAnyObjectByType<RouteManager>();

        if (_spawnOnStart)
            SpawnWave(_initialSpawnCount);
    }

    public void SpawnWave(int n)
    {
        for (int i = 0; i < n; i++)
            SpawnOne();
        StartCoroutine(DelaySendAllToGoal());
    }

    private IEnumerator DelaySendAllToGoal()
    {
        yield return null; 
        SendAllToGoal();
    }

    public MonsterMover SpawnOne()
    {
        if (!_monsterPrefab || !_map || !_route) return null;

        Vector2Int spawnRC = _route.SpawnCell; 
        Vector3 pos = CellToSpawnWorld(spawnRC.x, spawnRC.y);

        MonsterMover m = Instantiate(_monsterPrefab, pos, Quaternion.identity);
        m.Map = _map;                     
        _monsters.Add(m);
        return m;
    }

   
    public void SendAllToGoal()
    {
        if (!_route) return;
        foreach (MonsterMover m in _monsters)
        {
            if (!m) continue;
            m.MoveToCell(_route.GoalCell); 
        }
    }

    
    public void OnRouteChanged()
    {
        SendAllToGoal(); 
    }

    private Vector3 CellToSpawnWorld(int row, int col)
    {
        return _map.CellToWorld(row, col);
    }

}
