using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using UnityEngine;
using Utils; // AddressableManager / SimpleSingleton

public sealed class MonsterManager : MonoBehaviour
{
    private const string EVT_STAGE_LOADED = "EVT_STAGE_LOADED";

    public static MonsterManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private TestMap _map;
    [SerializeField] private RouteManager _route;

    [Header("Boss Spawn")]
    [SerializeField] private GameObject _bossPrefab;
    [SerializeField] private bool _spawnBossOnBase = true;
    private bool _bossSpawned = false;

    [Header("Wave (Manual / Auto)")]
    [Tooltip("수동 CSV 지정(우선 적용). 비어 있으면 Stage Id로 자동 로드")]
    [SerializeField] private TextAsset _csvWaveFile;
    [SerializeField] private bool _runCsvOnStart = false;

    [Tooltip("SelectedStage.Id로 Resources/<root>/<Id>.csv 자동 로드")]
    [SerializeField] private bool _autoLoadStageCsv = true;
    [Tooltip("CSV 루트(Resources 하위). 예: Monster/Table → Resources/Monster/Table/<Id>.csv")]
    [SerializeField] private string _csvResourcesRoot = "Monster/Table";

    [Header("Pooling / Prewarm")]
    [Tooltip("게임 시작 시 CSV를 기준으로 미리 생성할지 여부")]
    [SerializeField] private bool _prewarmOnStart = true;
    [Tooltip("CSV 외의 스폰에도 대비할 기본 프리웜 수(프리팹당)")]
    [SerializeField] private int _defaultPrewarmCount = 8;
    [Tooltip("프리웜 시 한 프레임에 생성할 최대 개수(스파이크 방지)")]
    [SerializeField] private int _prewarmPerFrame = 8;
    [Tooltip("풀 오브젝트들을 담아둘 부모(없으면 이 오브젝트)")]
    [SerializeField] private Transform _poolContainer;

    private readonly List<MonsterMover> _monsters = new List<MonsterMover>();
    private readonly Dictionary<string, MonsterMover> _prefabCache = new Dictionary<string, MonsterMover>(); // (monsterId → prefab) 기존 유지
    private readonly Dictionary<MonsterMover, (bool allowWalls, bool allowTowers)> _policy = new Dictionary<MonsterMover, (bool allowWalls, bool allowTowers)>();
    private readonly Dictionary<MonsterMover, Stack<MonsterMover>> _pool = new Dictionary<MonsterMover, Stack<MonsterMover>>();
    private readonly Queue<PooledMonster> _pendingReturn = new Queue<PooledMonster>();

    private Coroutine _scheduleCo;
    private Coroutine _prewarmCo;

    // Addressables
    private AddressableManager _addrMgr;
    private readonly Dictionary<string, MonsterMover> _addrCache = new Dictionary<string, MonsterMover>(); // (address → prefab)

    // 중복 실행 방지용
    private bool _didPrewarm = false;
    private bool _didSchedule = false;

    public List<MonsterMover> Monsters { get { return _monsters; } }

    [Serializable]
    public sealed class CsvSpawnRow
    {
        public string monsterId;
        public string prefabPath;
        public int count = 1;
        public float interval = 0f;
        public float delay = 0f;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_poolContainer == null)
        {
            _poolContainer = this.transform;
        }
    }

    private void OnEnable()
    {
        EventManager.Instance?.Subscribe<string>(EVT_STAGE_LOADED, OnStageLoaded);
    }

    private void OnDisable()
    {
        EventManager.Instance?.UnSubscribe(EVT_STAGE_LOADED, (Action<string>)OnStageLoaded);
    }

    private void Start()
    {
        if (_addrMgr == null) { _addrMgr = SimpleSingleton<AddressableManager>.Instance; }

        if (_map == null) { _map = FindAnyObjectByType<TestMap>(); }
        if (_route == null) { _route = FindAnyObjectByType<RouteManager>(); }

        TextAsset csv = _csvWaveFile;
        if (csv == null && _autoLoadStageCsv)
        {
            csv = ResolveStageCsvFromSelected();
        }

        // 프리웜을 쓴다면: Addressables 프리로드 후 프리웜
        if (_prewarmOnStart && csv != null)
        {
            StartCoroutine(CoPreloadThenPrewarm(csv));
            _didPrewarm = true;
        }

        if (_runCsvOnStart && csv != null)
        {
            StartScheduleFromCsv(csv);
            _didSchedule = true;
        }
        else if (_runCsvOnStart && csv == null)
        {
            Debug.LogWarning("[MonsterManager] 시작 시 로드할 CSV를 찾지 못했습니다. (_csvWaveFile 비었고, Stage Id 기반 자동 로드 실패)");
        }
    }

    private IEnumerator CoPreloadThenPrewarm(TextAsset csvFile)
    {
        List<CsvSpawnRow> rows = ParseCsv(csvFile);
        yield return PreloadMonstersFromCsv(rows);
        StartPrewarmFromCsv(csvFile, _defaultPrewarmCount);
    }

    private void LateUpdate()
    {
        while (_pendingReturn.Count > 0)
        {
            PooledMonster pm = _pendingReturn.Dequeue();
            if (pm == null) { continue; }

            MonsterMover m = pm.GetComponent<MonsterMover>();
            if (m == null || pm.PrefabKey == null) { continue; }

            InternalReturn(m, pm.PrefabKey, true);
        }
    }

    private void OnStageLoaded(string stageId)
    {
        if (_map == null) { _map = FindAnyObjectByType<TestMap>(); }
        if (_route == null) { _route = FindAnyObjectByType<RouteManager>(); }

        _bossSpawned = false;

        if (_route != null) { _route.RebuildAndApply(true); }
        TrySpawnBossAtBase();

        TextAsset csv = _csvWaveFile;
        if (csv == null && _autoLoadStageCsv)
        {
            csv = ResolveStageCsvFromSelected();
        }

        if (_prewarmOnStart && !_didPrewarm && csv != null)
        {
            StartCoroutine(CoPreloadThenPrewarm(csv));
            _didPrewarm = true;
        }

        if (_runCsvOnStart && !_didSchedule && csv != null)
        {
            StartScheduleFromCsv(csv);
            _didSchedule = true;
        }
    }

    /* =========================
     *  CSV 자동 로드 (Resources/Monster/Table/<StageId>.csv)
     * ========================= */
    private TextAsset ResolveStageCsvFromSelected()
    {
        NormalStageManager nsm = null;
        try { nsm = NormalStageManager.Instance; } catch { }

        if (nsm == null)
        {
            Debug.LogWarning("[MonsterManager] NormalStageManager 인스턴스를 찾을 수 없습니다.");
            return null;
        }

        NormalStageData stage = nsm.SelectedStage;

        string root = _csvResourcesRoot != null ? _csvResourcesRoot.Trim().TrimEnd('/', '\\') : string.Empty;
        string path = string.IsNullOrEmpty(root) ? stage.Id : (root + "/" + stage.Id);

        TextAsset csv = Resources.Load<TextAsset>(path);
        if (csv == null)
        {
            Debug.LogWarning("[MonsterManager] CSV 로드 실패: Resources/" + path + ".csv 가 존재해야 합니다.");
            return null;
        }
        return csv;
    }

    /* =========================
     *  스케줄 실행
     * ========================= */
    public void StartScheduleFromCsv(TextAsset csvFile)
    {
        List<CsvSpawnRow> rows = ParseCsv(csvFile);
        if (rows.Count == 0)
        {
            Debug.LogWarning("[MonsterManager] CSV가 비었거나 파싱 실패.");
            return;
        }

        if (_scheduleCo != null) { StopCoroutine(_scheduleCo); }
        _scheduleCo = StartCoroutine(RunScheduleCsv(rows));
    }

    public void StopSchedule()
    {
        if (_scheduleCo != null) { StopCoroutine(_scheduleCo); }
        _scheduleCo = null;
    }

    public void OnRouteChanged()
    {
        if (_route == null) { return; }

        RouteManager.RouteAllowance allowance = _route.Allowance;
        bool allowWalls = (allowance == RouteManager.RouteAllowance.WallsOnly
                        || allowance == RouteManager.RouteAllowance.WallsAndTowers);
        bool allowTowers = (allowance == RouteManager.RouteAllowance.WallsAndTowers);

        for (int i = 0; i < _monsters.Count; i++)
        {
            MonsterMover m = _monsters[i];
            if (m == null) { continue; }
            if (!m.gameObject.activeInHierarchy) { continue; }
            SendToGoal(m);
        }
    }

    private IEnumerator RunScheduleCsv(IList<CsvSpawnRow> rows)
    {
        if (_map == null || _route == null) { yield break; }

        // Addressables 프리로드(카탈로그 없이 CSV 주소 그대로)
        yield return PreloadMonstersFromCsv(rows);

        yield return new WaitUntil(() => MapManager.Instance != null && MapManager.Instance.HasPlayerBase);

        TrySpawnBossAtBase();
        _route.RebuildAndApply(true);

        for (int ri = 0; ri < rows.Count; ri++)
        {
            CsvSpawnRow row = rows[ri];
            if (row == null || string.IsNullOrWhiteSpace(row.prefabPath) || row.count <= 0) { continue; }

            if (row.delay > 0f) { yield return new WaitForSeconds(row.delay); }

            MonsterMover prefab = GetOrLoadPrefab(row.monsterId, row.prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning("[MonsterManager] Prefab load 실패: id='" + row.monsterId + "', path='" + row.prefabPath + "'");
                continue;
            }

            for (int i = 0; i < row.count; i++)
            {
                MonsterMover m = SpawnAtRC(prefab, _route.SpawnCell, true);
                if (row.interval > 0f) { yield return new WaitForSeconds(row.interval); }
            }
        }

        _scheduleCo = null;
    }

    /* =========================
     *  Addressables 프리로드
     * ========================= */
    private IEnumerator PreloadMonstersFromCsv(IList<CsvSpawnRow> rows)
    {
        if (rows == null) { yield break; }
        if (_addrMgr == null) { _addrMgr = SimpleSingleton<AddressableManager>.Instance; }

        HashSet<string> need = new HashSet<string>();
        for (int i = 0; i < rows.Count; i++)
        {
            string addr = DeriveAddress(rows[i]);
            if (!string.IsNullOrWhiteSpace(addr) && !_addrCache.ContainsKey(addr))
            {
                need.Add(addr);
            }
        }

        foreach (string addr in need)
        {
            Task<GameObject> t = _addrMgr.GetAddressableAsset<GameObject>(addr);
            while (!t.IsCompleted) { yield return null; }

            MonsterMover prefab = null;
            GameObject go = t.Result;
            if (go != null) { prefab = go.GetComponent<MonsterMover>(); }

            if (prefab != null) { _addrCache[addr] = prefab; }
            else { Debug.LogWarning("[MonsterManager] Addressables load failed or component missing: " + addr); }
        }
    }

    private static string DeriveAddress(CsvSpawnRow row)
    {
        if (row == null) { return null; }
        if (!string.IsNullOrWhiteSpace(row.prefabPath)) { return row.prefabPath.Trim(); }
        if (!string.IsNullOrWhiteSpace(row.monsterId)) { return row.monsterId.Trim(); }
        return null;
    }

    /* =========================
     *  프리팹 로드 / 캐시
     * ========================= */
    private MonsterMover GetOrLoadPrefab(string monsterId, string resourcesPath)
    {
        // 1) Addressables 캐시 우선 (prefabPath=Address 권장)
        string addr = !string.IsNullOrWhiteSpace(resourcesPath) ? resourcesPath : monsterId;
        if (!string.IsNullOrWhiteSpace(addr))
        {
            MonsterMover cachedAddr;
            if (_addrCache.TryGetValue(addr, out cachedAddr) && cachedAddr != null)
            {
                return cachedAddr;
            }
        }

        // 2) 기존 캐시(monsterId → prefab)도 유지
        if (!string.IsNullOrWhiteSpace(monsterId))
        {
            MonsterMover cached;
            if (_prefabCache.TryGetValue(monsterId, out cached) && cached != null)
            {
                return cached;
            }
        }

        // 3) 폴백: Resources (이행기 안정성; Addressables-only로 강제하려면 아래 블록 제거)
        MonsterMover loaded = null;
        if (!string.IsNullOrWhiteSpace(resourcesPath))
        {
            loaded = Resources.Load<MonsterMover>(resourcesPath);
        }

        if (loaded != null && !string.IsNullOrWhiteSpace(monsterId))
        {
            _prefabCache[monsterId] = loaded;
        }
        return loaded;
    }

    /* =========================
     *  프리웜 / 풀링
     * ========================= */
    public void StartPrewarmFromCsv(TextAsset csvFile, int fallbackEach)
    {
        if (_prewarmCo != null) { StopCoroutine(_prewarmCo); }

        Dictionary<MonsterMover, int> plan = BuildPrewarmPlan(csvFile, fallbackEach);
        _prewarmCo = StartCoroutine(PrewarmCoroutine(plan, _prewarmPerFrame));
    }

    public void EnqueueReturn(PooledMonster pm)
    {
        if (pm != null)
        {
            _pendingReturn.Enqueue(pm);
        }
    }

    private Dictionary<MonsterMover, int> BuildPrewarmPlan(TextAsset csvFile, int fallbackEach)
    {
        Dictionary<MonsterMover, int> plan = new Dictionary<MonsterMover, int>();

        if (csvFile != null)
        {
            List<CsvSpawnRow> rows = ParseCsv(csvFile);
            for (int i = 0; i < rows.Count; i++)
            {
                CsvSpawnRow row = rows[i];
                if (row == null || string.IsNullOrWhiteSpace(row.prefabPath) || row.count <= 0) { continue; }

                MonsterMover prefab = GetOrLoadPrefab(row.monsterId, row.prefabPath);
                if (prefab == null) { continue; }

                int want = row.count;
                int exist;
                if (plan.TryGetValue(prefab, out exist))
                {
                    int sum = exist + want;
                    plan[prefab] = sum;
                }
                else
                {
                    plan[prefab] = want;
                }
            }
        }

        if (fallbackEach > 0)
        {
            foreach (KeyValuePair<string, MonsterMover> kv in _prefabCache)
            {
                MonsterMover prefab = kv.Value;
                if (prefab == null) { continue; }
                if (!plan.ContainsKey(prefab)) { plan[prefab] = fallbackEach; }
            }
        }

        return plan;
    }

    private IEnumerator PrewarmCoroutine(Dictionary<MonsterMover, int> plan, int perFrame)
    {
        if (plan == null || plan.Count == 0) { yield break; }
        if (perFrame <= 0) { perFrame = 8; }

        foreach (KeyValuePair<MonsterMover, int> kv in plan)
        {
            MonsterMover prefab = kv.Key;
            int wantTotal = Mathf.Max(0, kv.Value);

            Stack<MonsterMover> stack;
            if (!_pool.TryGetValue(prefab, out stack))
            {
                stack = new Stack<MonsterMover>();
                _pool[prefab] = stack;
            }

            int have = stack.Count;
            int need = Mathf.Max(0, wantTotal - have);

            while (need > 0)
            {
                int batch = Mathf.Min(perFrame, need);
                for (int i = 0; i < batch; i++)
                {
                    MonsterMover created = CreateOneForPool(prefab);
                    stack.Push(created);
                }
                need -= batch;
                yield return null;
            }
        }
        _prewarmCo = null;
    }

    private MonsterMover CreateOneForPool(MonsterMover prefab)
    {
        Vector3 off = new Vector3(10000f, 10000f, 0f);
        MonsterMover m = Instantiate(prefab, off, Quaternion.identity);

        PooledMonster pooled = m.gameObject.GetComponent<PooledMonster>();
        if (pooled == null) { pooled = m.gameObject.AddComponent<PooledMonster>(); }
        pooled.Manager = this;
        pooled.PrefabKey = prefab;

        Animator animator = m.GetComponentInChildren<Animator>(true);
        if (animator != null) { animator.Rebind(); animator.Update(0f); }

        ParticleSystem ps = m.GetComponentInChildren<ParticleSystem>(true);
        if (ps != null) { ps.Simulate(0f, true, true); ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear); }

        pooled.SuppressReturnOnce();
        m.gameObject.SetActive(false);
        m.transform.SetParent(_poolContainer, false);
        return m;
    }

    public void DespawnToPool(MonsterMover m, MonsterMover prefabKey)
    {
        if (m == null || prefabKey == null) { return; }

        m.OnOwnerDied();
        InternalReturn(m, prefabKey, false);
    }

    private void InternalReturn(MonsterMover m, MonsterMover prefabKey, bool alreadyInactive)
    {
        _monsters.Remove(m);
        _policy.Remove(m);

        Stack<MonsterMover> stack;
        if (!_pool.TryGetValue(prefabKey, out stack))
        {
            stack = new Stack<MonsterMover>();
            _pool[prefabKey] = stack;
        }

        if (!alreadyInactive && m.gameObject.activeSelf)
        {
            PooledMonster pooled = m.GetComponent<PooledMonster>();
            if (pooled != null) { pooled.SuppressReturnOnce(); }
            m.gameObject.SetActive(false);
        }

        if (_poolContainer != null)
        {
            m.transform.SetParent(_poolContainer, false);
        }
        stack.Push(m);
    }

    /* =========================
     *  스폰/이동
     * ========================= */
    public MonsterMover SpawnAtRC(MonsterMover prefab, Vector2Int rc, bool sendToGoal = true)
    {
        if (prefab == null || _map == null) { return null; }
        Vector3 world = _map.CellToWorld(rc.x, rc.y);
        world.z = 0f;
        return SpawnAtWorld(prefab, world, sendToGoal);
    }

    public MonsterMover SpawnAtWorld(MonsterMover prefab, Vector3 world, bool sendToGoal = true)
    {
        if (prefab == null || _map == null) { return null; }

        Stack<MonsterMover> stack;
        if (!_pool.TryGetValue(prefab, out stack))
        {
            stack = new Stack<MonsterMover>();
            _pool[prefab] = stack;
        }

        MonsterMover m = null;
        while (stack.Count > 0 && m == null)
        {
            m = stack.Pop();
        }

        if (m == null)
        {
            m = Instantiate(prefab, world, Quaternion.identity);
            PooledMonster pooledNew = m.gameObject.GetComponent<PooledMonster>();
            if (pooledNew == null) { pooledNew = m.gameObject.AddComponent<PooledMonster>(); }
            pooledNew.Manager = this;
            pooledNew.PrefabKey = prefab;
        }
        else
        {
            m.transform.SetPositionAndRotation(world, Quaternion.identity);
        }

        if (!m.gameObject.activeSelf) { m.gameObject.SetActive(true); }
        if (_map == null) { _map = FindAnyObjectByType<TestMap>(); }
        m.Map = _map;

        Vector2Int rc = _map.WorldToCell(world);
        m.SetCellAndSnap(rc);

        if (!_monsters.Contains(m)) { _monsters.Add(m); }

        if (sendToGoal) { SendToGoalResilient(m); }

        return m;
    }

    private void SendToGoal(MonsterMover m)
    {
        if (m == null || _route == null) { return; }

        RouteManager.RouteAllowance allowance = _route.Allowance;
        bool allowWalls = (allowance == RouteManager.RouteAllowance.WallsOnly
                        || allowance == RouteManager.RouteAllowance.WallsAndTowers);
        bool allowTowers = (allowance == RouteManager.RouteAllowance.WallsAndTowers);

        (bool allowWalls, bool allowTowers) pol;
        if (_policy.TryGetValue(m, out pol))
        {
            allowWalls = pol.allowWalls || allowWalls;
            allowTowers = pol.allowTowers || allowTowers;
        }

        m.MoveToCell(_route.GoalCell, allowWalls, allowTowers);
    }

    private void SendToGoalResilient(MonsterMover m)
    {
        if (m == null || _route == null) { return; }

        RouteManager.RouteAllowance allowance = _route.Allowance;
        bool allowWallsBase = (allowance == RouteManager.RouteAllowance.WallsOnly
                            || allowance == RouteManager.RouteAllowance.WallsAndTowers);
        bool allowTowersBase = (allowance == RouteManager.RouteAllowance.WallsAndTowers);

        (bool allowWalls, bool allowTowers) pol;
        if (_policy.TryGetValue(m, out pol))
        {
            m.MoveToCell(_route.GoalCell, pol.allowWalls, pol.allowTowers);
            if (m.IsFollowingPath) { return; }
        }

        m.MoveToCell(_route.GoalCell, allowWallsBase, allowTowersBase);
        if (m.IsFollowingPath) { return; }

        m.MoveToCell(_route.GoalCell, true, false);
        if (m.IsFollowingPath) { return; }

        m.MoveToCell(_route.GoalCell, true, true);
    }

    /* =========================
     *  유틸
     * ========================= */
    private void TrySpawnBossAtBase()
    {
        if (_bossSpawned) { return; }
        if (!_spawnBossOnBase) { return; }
        if (_bossPrefab == null || _map == null) { return; }

        Vector2Int rc = _map.HasBossSpawnCell ? _map.BossSpawnCellRC
                     : _map.HasSpawnCell ? _map.SpawnCellRC
                     : new Vector2Int(0, 0);

        Vector3 world = _map.CellToWorld(rc.x, rc.y);
        world.z = 0f;

        Instantiate(_bossPrefab, world, Quaternion.identity);
        _bossSpawned = true;
    }

    private List<CsvSpawnRow> ParseCsv(TextAsset csvFile)
    {
        List<CsvSpawnRow> list = new List<CsvSpawnRow>();
        if (csvFile == null) { return list; }

        string text = csvFile.text;
        if (string.IsNullOrWhiteSpace(text)) { return list; }

        string[] lines = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

        int start = 0;
        if (lines.Length > 0 && lines[0].ToLowerInvariant().Contains("monsterid")) { start = 1; }

        for (int i = start; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) { continue; }

            string[] parts = line.Split(',');
            if (parts.Length < 5) { continue; }

            CsvSpawnRow row = new CsvSpawnRow();
            row.monsterId = parts[0].Trim();
            row.prefabPath = parts[1].Trim();
            row.count = ParseInt(parts[2], 1);
            row.interval = ParseFloat(parts[3], 0f);
            row.delay = ParseFloat(parts[4], 0f);

            row.count = Mathf.Max(0, row.count);
            row.interval = Mathf.Max(0f, row.interval);
            row.delay = Mathf.Max(0f, row.delay);

            list.Add(row);
        }

        return list;
    }

    private static int ParseInt(string s, int def)
    {
        int v;
        return int.TryParse(s.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out v) ? v : def;
    }

    private static float ParseFloat(string s, float def)
    {
        float v;
        return float.TryParse(s.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out v) ? v : def;
    }
}
