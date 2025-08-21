using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TestMap map;
    [SerializeField] private RouteManager route;       
    [SerializeField] private MonsterMover monsterPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int initialSpawnCount = 5; 
    [SerializeField] private bool snapToCellCenter = true; 
    [SerializeField] private bool spawnOnStart = true;

    private readonly List<MonsterMover> _monsters = new List<MonsterMover>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        if (!map) map = FindAnyObjectByType<TestMap>();
        if (!route) route = FindAnyObjectByType<RouteManager>();

        if (spawnOnStart)
            SpawnWave(initialSpawnCount);
    }

   

    public void SpawnWave(int n)
    {
        for (int i = 0; i < n; i++)
            SpawnOne();
        SendAllToGoal();
    }

    public MonsterMover SpawnOne()
    {
        if (!monsterPrefab || !map || !route) return null;

        var spawnRC = route.SpawnCell; 
        var pos = CellToSpawnWorld(spawnRC.x, spawnRC.y);

        var m = Instantiate(monsterPrefab, pos, Quaternion.identity);
        m.Map = map;                     
        _monsters.Add(m);
        return m;
    }

   
    public void SendAllToGoal()
    {
        if (!route) return;
        foreach (var m in _monsters)
        {
            if (!m) continue;
            m.MoveToCell(route.GoalCell); 
        }
    }

    
    public void OnRouteChanged()
    {
        SendAllToGoal(); 
    }

    private Vector3 CellToSpawnWorld(int row, int col)
    {
        Vector3 basePos = map.CellToWorld(row, col);
        if (!snapToCellCenter) return basePos;

        float half = map.CellSize * 0.5f; 
        return basePos + new Vector3(half, half, 0f);
    }
}
