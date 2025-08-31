//using System.Collections.Generic;
//using UnityEngine;

//public class MonsterManager : MonoBehaviour
//{
//    public static MonsterManager Instance { get; private set; }

//    [Header("References")]
//    [SerializeField] private TestMap _map;
//    [SerializeField] private RouteManager _route;
//    [SerializeField] private MonsterMover _monsterPrefab;

//    [Header("Spawn Settings")]
//    [SerializeField] private int _initialSpawnCount = 5;
//    [SerializeField] private bool _snapToCellCenter = true;
//    [SerializeField] private bool _spawnOnStart = false;

//    private readonly List<MonsterMover> _monsters = new List<MonsterMover>();

//    private void Awake()
//    {
//        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
//        Instance = this;
//    }

//    private void OnEnable()
//    {
//        if (MapManager.Instance != null)
//            MapManager.Instance.OnPlayerBasePlaced += HandleBasePlaced;
//    }

//    private void OnDisable()
//    {
//        if (MapManager.Instance != null)
//            MapManager.Instance.OnPlayerBasePlaced -= HandleBasePlaced;
//    }

//    private void Start()
//    {
//        if (!_map) _map = FindAnyObjectByType<TestMap>();
//        if (!_route) _route = FindAnyObjectByType<RouteManager>();

//        if (_spawnOnStart && MapManager.Instance != null && MapManager.Instance.HasPlayerBase)
//        {

//            EnsureRouteReady();
//            SpawnWave(_initialSpawnCount, sendToGoalImmediately: true);
//        }
//    }


//    private void HandleBasePlaced(Vector3Int baseCell)
//    {
//        EnsureRouteReady();                        
//        SpawnWave(_initialSpawnCount, sendToGoalImmediately: false);       
//    }

//    private void EnsureRouteReady()
//    {
//        if (_route != null)
//            _route.RebuildAndApply(force: true);    
//    }


//    public MonsterMover SpawnOne()
//    {
//        if (!_monsterPrefab || !_map || !_route) return null;
//        if (MapManager.Instance != null && !MapManager.Instance.HasPlayerBase) return null; 

//        Vector2Int spawnRC = _route.SpawnCell;
//        Vector3 pos = _map.CellToWorld(spawnRC.x, spawnRC.y);
//        if (_snapToCellCenter) pos.z = 0f;

//        MonsterMover m = Instantiate(_monsterPrefab, pos, Quaternion.identity);
//        m.Map = _map;
//        m.SetCellAndSnap(_route.SpawnCell);
//        _monsters.Add(m);
//        return m;
//    }

//    public void SpawnWave(int n, bool sendToGoalImmediately = false)
//    {
//        if (n <= 0) return;

//        for (int i = 0; i < n; i++)
//            SpawnOne();

//        if (sendToGoalImmediately)
//            SendAllToGoal();
//    }

//    public void SendAllToGoal()
//    {
//        if (!_route) return;

//        var allowance = _route.Allowance; // RouteAllowance

//        foreach (MonsterMover m in _monsters)
//        {
//            if (!m) continue;
//            bool allowWalls = (allowance == RouteManager.RouteAllowance.WallsOnly || allowance == RouteManager.RouteAllowance.WallsAndTowers);
//            bool allowTowers = (allowance == RouteManager.RouteAllowance.WallsAndTowers);

//            m.MoveToCell(_route.GoalCell, allowWalls, allowTowers);
//        }
//    }


//    public void OnRouteChanged()
//    {
//        SendAllToGoal();
//    }
//}


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

    [Header("CSV Wave")]
    [Tooltip("MonsterId,Prefab,Count,Interval,Delay 헤더를 가진 CSV(TextAsset)")]
    [SerializeField] private TextAsset _csvWaveFile;
    [Tooltip("Start에서 CSV를 자동 실행할지 여부")]
    [SerializeField] private bool _runCsvOnStart = false;

    private readonly List<MonsterMover> _monsters = new();
    private readonly Dictionary<string, MonsterMover> _prefabCache = new();

    private Coroutine _scheduleCo;

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
            m.MoveToCell(_route.GoalCell, allowWalls, allowTowers);
        }
    }

    private IEnumerator RunScheduleCsv(IList<CsvSpawnRow> rows)
    {
        if (_map == null || _route == null) yield break;

        yield return new WaitUntil(() => MapManager.Instance != null && MapManager.Instance.HasPlayerBase);


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
                MonsterMover m = SpawnOneOf(prefab);

                yield return null;
                SendToGoal(m);

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

    private MonsterMover SpawnOneOf(MonsterMover prefab)
    {
        if (!prefab || !_map || !_route) return null;
        if (MapManager.Instance != null && !MapManager.Instance.HasPlayerBase) return null;

        Vector2Int rc = _route.SpawnCell;
        Vector3 pos = _map.CellToWorld(rc.x, rc.y);
        pos.z = 0f;

        MonsterMover m = Instantiate(prefab, pos, Quaternion.identity);
        m.Map = _map;

        _monsters.Add(m);
        return m;
    }

    private void SendToGoal(MonsterMover m)
    {
        if (!m || _route == null) return;

        RouteManager.RouteAllowance allowance = _route.Allowance;
        bool allowWalls = (allowance == RouteManager.RouteAllowance.WallsOnly
                         || allowance == RouteManager.RouteAllowance.WallsAndTowers);
        bool allowTowers = (allowance == RouteManager.RouteAllowance.WallsAndTowers);

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

    private static int ParseInt(string s, int def)
        => int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) ? v : def;

    private static float ParseFloat(string s, float def)
        => float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : def;
}
