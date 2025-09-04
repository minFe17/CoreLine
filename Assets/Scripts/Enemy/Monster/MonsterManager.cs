using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    public static MonsterManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TestMap _map;
    [SerializeField] private RouteManager _route;

    [Header("Boss Spawn")]
    [SerializeField] private GameObject _bossPrefab;    
    [SerializeField] private bool _spawnBossOnBase = true;
    private bool _bossSpawned = false;

    [Header("Wave")]
    [SerializeField] private TextAsset _csvWaveFile;
    [SerializeField] private bool _runCsvOnStart = false;

    private readonly List<MonsterMover> _monsters = new();
    private readonly Dictionary<string, MonsterMover> _prefabCache = new();

    private readonly Dictionary<MonsterMover, (bool allowWalls, bool allowTowers)> _policy = new();
    private readonly Dictionary<MonsterMover, Stack<MonsterMover>> _pool = new();

    private Coroutine _scheduleCo;

    public List<MonsterMover> Monsters { get => _monsters; }

    [Serializable]
    public class CsvSpawnRow
    {
        public string monsterId;   
        public string prefabPath;  
        public int count = 1;
        public float interval = 0f;
        public float delay = 0f;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (!_map) _map = FindAnyObjectByType<TestMap>();
        if (!_route) _route = FindAnyObjectByType<RouteManager>();

        if (_runCsvOnStart && _csvWaveFile != null)
            StartScheduleFromCsv(_csvWaveFile);
    }

   
    public void StartScheduleFromCsv(TextAsset csvFile)
    {
        List<CsvSpawnRow> rows = ParseCsv(csvFile);
        if (rows.Count == 0)
        {
            Debug.LogWarning("[MonsterManager] CSV가 비었거나 파싱 실패.");
            return;
        }

        if (_scheduleCo != null) StopCoroutine(_scheduleCo);
        _scheduleCo = StartCoroutine(RunScheduleCsv(rows));
    }

    public void StopSchedule()
    {
        if (_scheduleCo != null) StopCoroutine(_scheduleCo);
        _scheduleCo = null;
    }

    public void OnRouteChanged()
    {
        if (_route == null) return;

        RouteManager.RouteAllowance allowance = _route.Allowance;
        bool allowWalls = (allowance == RouteManager.RouteAllowance.WallsOnly
                         || allowance == RouteManager.RouteAllowance.WallsAndTowers);
        bool allowTowers = (allowance == RouteManager.RouteAllowance.WallsAndTowers);

        foreach (MonsterMover m in _monsters)
        {
            if (!m) continue;
            if (!m.gameObject.activeInHierarchy) continue;
            //m.MoveToCell(_route.GoalCell, allowWalls, allowTowers);
            SendToGoal(m);
        }
    }

    private IEnumerator RunScheduleCsv(IList<CsvSpawnRow> rows)
    {
        if (_map == null || _route == null) yield break;

        yield return new WaitUntil(() => MapManager.Instance != null && MapManager.Instance.HasPlayerBase);

        TrySpawnBossAtBase();

        _route.RebuildAndApply(force: true);

        for (int ri = 0; ri < rows.Count; ri++)
        {
            CsvSpawnRow row = rows[ri];
            if (row == null || string.IsNullOrWhiteSpace(row.prefabPath) || row.count <= 0)
                continue;

            if (row.delay > 0f)
                yield return new WaitForSeconds(row.delay);

            MonsterMover prefab = GetOrLoadPrefab(row.monsterId, row.prefabPath);
            if (!prefab)
            {
                Debug.LogWarning($"[MonsterManager] Prefab load 실패: id='{row.monsterId}', path='{row.prefabPath}'");
                continue;
            }

            for (int i = 0; i < row.count; i++)
            {
                MonsterMover m = SpawnAtRC(prefab, _route.SpawnCell, sendToGoal: true);

                if (row.interval > 0f)
                    yield return new WaitForSeconds(row.interval);
            }
        }

        _scheduleCo = null;
    }

    private MonsterMover GetOrLoadPrefab(string monsterId, string resourcesPath)
    {
        if (!string.IsNullOrWhiteSpace(monsterId) && _prefabCache.TryGetValue(monsterId, out var cached) && cached)
            return cached;

        MonsterMover loaded = Resources.Load<MonsterMover>(resourcesPath);
        if (loaded && !string.IsNullOrWhiteSpace(monsterId))
            _prefabCache[monsterId] = loaded;

        return loaded;

        
    }

    private void SendToGoal(MonsterMover m)
    {
        if (!m || _route == null) return;

        RouteManager.RouteAllowance allowance = _route.Allowance;
        bool allowWalls = (allowance == RouteManager.RouteAllowance.WallsOnly
                         || allowance == RouteManager.RouteAllowance.WallsAndTowers);
        bool allowTowers = (allowance == RouteManager.RouteAllowance.WallsAndTowers);

        if (_policy.TryGetValue(m, out var pol))
        {
            
            allowWalls = pol.allowWalls || allowWalls;   
            allowTowers = pol.allowTowers || allowTowers;
        }

        m.MoveToCell(_route.GoalCell, allowWalls, allowTowers);
    }

    private List<CsvSpawnRow> ParseCsv(TextAsset csvFile)
    {
        List<CsvSpawnRow> list = new List<CsvSpawnRow>();
        if (csvFile == null) return list;

        string text = csvFile.text;
        if (string.IsNullOrWhiteSpace(text)) return list;

        string[] lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        int start = 0;
        if (lines.Length > 0 && lines[0].ToLowerInvariant().Contains("monsterid"))
            start = 1;

        for (int i = start; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(',');
            if (parts.Length < 5) continue;

            CsvSpawnRow row = new CsvSpawnRow
            {
                monsterId = parts[0].Trim(),
                prefabPath = parts[1].Trim(),
                count = ParseInt(parts[2], 1),
                interval = ParseFloat(parts[3], 0f),
                delay = ParseFloat(parts[4], 0f),
            };
            row.count = Mathf.Max(0, row.count);
            row.interval = Mathf.Max(0f, row.interval);
            row.delay = Mathf.Max(0f, row.delay);

            list.Add(row);
        }

        return list;
    }

    
    private void TrySpawnBossAtBase()
    {
        if (_bossSpawned) return;
        if (!_spawnBossOnBase) return;
        if (_bossPrefab == null || _map == null) return;

        Vector2Int rc = _map.HasBossSpawnCell ? _map.BossSpawnCellRC
                     : _map.HasSpawnCell ? _map.SpawnCellRC
                     : new Vector2Int(0, 0);

        Vector3 world = _map.CellToWorld(rc.x, rc.y);
        world.z = 0f;

        Instantiate(_bossPrefab, world, Quaternion.identity);
        _bossSpawned = true;
    }

    public GameObject SpawnEnemyAt(
    GameObject prefab, Vector3 world,
    bool? allowWallsOverride = null,
    bool? allowTowersOverride = null,
    bool sendToGoal = true)
    {
        if (!prefab) return null;

        var moverPrefab = prefab.GetComponent<MonsterMover>();
        if (!moverPrefab) { Instantiate(prefab, world, Quaternion.identity); return null; }

        MonsterMover m;
        if (allowWallsOverride.HasValue || allowTowersOverride.HasValue)
        {
            m = SpawnAtWorld(moverPrefab, world, false);
            _policy[m] = (allowWallsOverride ?? false, allowTowersOverride ?? false);
            if (sendToGoal) SendToGoal(m);
        }
        else
        {
            m = SpawnAtWorld(moverPrefab, world, sendToGoal);
        }
        return m ? m.gameObject : null;
    }

    public void DespawnToPool(MonsterMover m, MonsterMover prefabKey)
    {
        if (!m || !prefabKey) return;

        m.OnOwnerDied();

        _monsters.Remove(m);
        _policy.Remove(m);

        if (!_pool.TryGetValue(prefabKey, out var stack))
            _pool[prefabKey] = stack = new Stack<MonsterMover>();

        stack.Push(m);
    }

    public MonsterMover SpawnAtRC(MonsterMover prefab, Vector2Int rc, bool sendToGoal = true)
    {
        if (!prefab || _map == null) return null;
        Vector3 world = _map.CellToWorld(rc.x, rc.y);
        world.z = 0f;
        return SpawnAtWorld(prefab, world, sendToGoal);
    }

    public MonsterMover SpawnAtWorld(MonsterMover prefab, Vector3 world, bool sendToGoal = true)
    {
        if (!prefab || _map == null) return null;

        // 풀에서 꺼내기
        if (!_pool.TryGetValue(prefab, out var stack))
            _pool[prefab] = stack = new Stack<MonsterMover>();

        MonsterMover m = null;
        while (stack.Count > 0 && !m) m = stack.Pop(); 

        if (m == null)
        {
            // 부족하면 새로 생성
            m = Instantiate(prefab, world, Quaternion.identity);
            var pooled = m.gameObject.GetComponent<PooledMonster>();
            if (!pooled) pooled = m.gameObject.AddComponent<PooledMonster>();
            pooled.Manager = this;
            pooled.PrefabKey = prefab;
        }
        else
        {
            // 재사용 세팅
            m.transform.SetPositionAndRotation(world, Quaternion.identity);
        }

        // 맵 연결
        if (!m.gameObject.activeSelf) m.gameObject.SetActive(true);
        if (!_map) _map = FindAnyObjectByType<TestMap>();
        m.Map = _map;

        Vector2Int rc = _map.WorldToCell(world);
        m.SetCellAndSnap(rc);

        if (!_monsters.Contains(m)) 
            _monsters.Add(m);

        if (sendToGoal) 
            SendToGoalRE(m);

        return m;
    }

    private void SendToGoalRE(MonsterMover m)
    {
        if (!m || _route == null) return;

        var allowance = _route.Allowance;
        bool allowWallsBase = (allowance == RouteManager.RouteAllowance.WallsOnly
                             || allowance == RouteManager.RouteAllowance.WallsAndTowers);
        bool allowTowersBase = (allowance == RouteManager.RouteAllowance.WallsAndTowers);

        if (_policy.TryGetValue(m, out var pol))
        {
            m.MoveToCell(_route.GoalCell, pol.allowWalls, pol.allowTowers);
            if (m.IsFollowingPath) return;
        }
        m.MoveToCell(_route.GoalCell, allowWallsBase, allowTowersBase);
        if (m.IsFollowingPath) return;

        m.MoveToCell(_route.GoalCell, true, false);
        if (m.IsFollowingPath) return;

        m.MoveToCell(_route.GoalCell, true, true);
    }



    private static int ParseInt(string s, int def)
        => int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : def;

    private static float ParseFloat(string s, float def)
        => float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : def;
}
