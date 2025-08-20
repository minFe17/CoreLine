using System.Collections.Generic;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TestMap map;
    [SerializeField] private RouteManager route;        // RouteManager를 인스펙터에 연결
    [SerializeField] private MonsterMover monsterPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int initialSpawnCount = 5; // 시작 시 한 번에 몇 마리 소환할지
    [SerializeField] private bool snapToCellCenter = true; // 셀 중앙으로 스폰할지
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

        var spawnRC = route.spawnCell; 
        var pos = CellToSpawnWorld(spawnRC.x, spawnRC.y);

        var m = Instantiate(monsterPrefab, pos, Quaternion.identity);
        m.map = map;                     
        _monsters.Add(m);
        return m;
    }

   
    public void SendAllToGoal()
    {
        if (!route) return;
        foreach (var m in _monsters)
        {
            if (!m) continue;
            m.MoveToCell(route.goalCell); // 현재 Cell → goal 로 길찾기
        }
    }

    
    public void OnRouteChanged()
    {
        SendAllToGoal(); // 라인이 바뀐 경우에만 다시 길찾기
    }

    // ========== Helpers ==========

    private Vector3 CellToSpawnWorld(int row, int col)
    {
        Vector3 basePos = map.CellToWorld(row, col);
        if (!snapToCellCenter) return basePos;

        float half = map.CellSize * 0.5f; 
        return basePos + new Vector3(half, half, 0f);
    }
}
