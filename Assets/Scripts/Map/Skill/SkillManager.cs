using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public partial class SkillManager : MonoSingleton<SkillManager>
{
    // 타입 정의
    public struct SelectedSkill
    {
        public string Id;
        public int Cost;
        public Effect Effect;

        public SelectedSkill(LaboratoryData def)
        {
            Id = def.Id;
            Cost = def.Cost;
            Effect = def.Effect;
        }
    }

    [Flags]
    public enum TargetKind
    {
        Towers = 1 << 0,
        Monsters = 1 << 1,
        Both = Towers | Monsters
    }

    // 이벤트
    public event Action OnLoadoutChanged;
    public event Action<IReadOnlyList<string>> OnLoadoutIdsChanged;
    public event Action OnDatabaseChanged;
    public event Action<IReadOnlyList<string>> OnUnlockedSkillIdsChanged;

    // 필드
    private List<LaboratoryData> _databaseList = new List<LaboratoryData>();
    private bool _autoLoadFromDataManager = true;
    private int _maxSlots = 3;
    private readonly List<SelectedSkill> _loadout = new List<SelectedSkill>();

    private Transform _unitRoot = null;
    private string _monsterTag = "Monster";

    private readonly Dictionary<string, LaboratoryData> _defsById = new Dictionary<string, LaboratoryData>();
    private readonly Dictionary<string, ITowerSkillHandler> _towerHandlers = new Dictionary<string, ITowerSkillHandler>();
    private readonly Dictionary<string, IMonsterSkillHandler> _monsterHandlers = new Dictionary<string, IMonsterSkillHandler>();
    private readonly Dictionary<TargetType, IIncomeSkillHandler> _incomeHandlers = new Dictionary<TargetType, IIncomeSkillHandler>();
    private readonly Dictionary<string, ISkillTargetingSpecProvider> _targetingProviders = new Dictionary<string, ISkillTargetingSpecProvider>();

    private IMonsterSkillHandler _defaultMonsterHandler = null;

    private readonly List<string> _loadoutIdsCache = new List<string>();
    private readonly List<string> _unlockedIdsCache = new List<string>();

    // 공개 프로퍼티
    public int MaxSlots => _maxSlots;
    public IReadOnlyList<SelectedSkill> Loadout => _loadout;
    public IReadOnlyList<LaboratoryData> Database => _databaseList;

    // 공개 설정 API
    public void Configure(int maxSlots = 3, bool autoLoadFromDataManager = true, string monsterTag = "Monster", Transform unitRoot = null)
    {
        _maxSlots = Mathf.Max(1, maxSlots);
        _autoLoadFromDataManager = autoLoadFromDataManager;
        _monsterTag = string.IsNullOrEmpty(monsterTag) ? "Monster" : monsterTag;
        _unitRoot = unitRoot;
    }

    public void SetDatabase(List<LaboratoryData> newList, bool rebuildIndex = true, bool raiseEvent = true)
    {
        _databaseList = FilterUnlocked(newList);
        if (rebuildIndex)
        {
            RebuildDefinitionIndex();
            RebuildUnlockedIdsCache();
        }
        if (raiseEvent)
        {
            OnDatabaseChanged?.Invoke();
            OnUnlockedSkillIdsChanged?.Invoke(_unlockedIdsCache.AsReadOnly());
        }
    }

    public IReadOnlyList<LaboratoryData> GetAllSkillDefs()
    {
        return _databaseList ?? (IReadOnlyList<LaboratoryData>)Array.Empty<LaboratoryData>();
    }

    public List<LaboratoryData> GetSkillDefsByType(LaboratoryType type)
    {
        List<LaboratoryData> list = new List<LaboratoryData>();
        if (_databaseList == null) return list;
        for (int i = 0; i < _databaseList.Count; i++)
        {
            LaboratoryData def = _databaseList[i];
            if (def.LaboratoryType == type) list.Add(def);
        }
        return list;
    }

    public bool TryGetSkillDef(string id, out LaboratoryData def)
    {
        return _defsById.TryGetValue(id, out def);
    }

    // 공개 로드아웃 API (ID 기반)
    public string[] GetLoadoutIdsSnapshot()
    {
        string[] arr = new string[_loadout.Count];
        for (int i = 0; i < _loadout.Count; i++) arr[i] = _loadout[i].Id;
        return arr;
    }

    public void SetLoadoutByIds(IEnumerable<string> ids, bool clearExisting = true, bool enforceUnique = true)
    {
        if (ids == null) return;
        if (clearExisting) _loadout.Clear();

        HashSet<string> seen = enforceUnique ? new HashSet<string>() : null;
        int added = 0;

        foreach (string id in ids)
        {
            if (string.IsNullOrEmpty(id)) continue;
            if (_loadout.Count >= _maxSlots) break;

            if (enforceUnique)
            {
                if (seen.Contains(id)) continue;
                if (_loadout.Exists(s => s.Id == id)) { seen.Add(id); continue; }
                seen.Add(id);
            }

            LaboratoryData def;
            if (!TryGetSkillDef(id, out def)) continue;

            _loadout.Add(new SelectedSkill(def));
            added++;
        }

        if (added > 0) RaiseLoadoutChanged();
    }

    public bool ContainsInLoadout(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        return _loadout.Exists(s => s.Id == id);
    }

    public bool TryGetLoadoutIdAt(int slotIndex, out string id)
    {
        id = null;
        if (slotIndex < 0 || slotIndex >= _loadout.Count) return false;
        id = _loadout[slotIndex].Id;
        return true;
    }

    public bool AddToLoadout(LaboratoryData def)
    {
        if (_loadout.Count >= _maxSlots)
        {
            Debug.LogWarning("[SkillManager] Loadout is full");
            return false;
        }

        _loadout.Add(new SelectedSkill(def));
        RaiseLoadoutChanged();
        return true;
    }

    public bool AddToLoadoutById(string id)
    {
        LaboratoryData def;
        if (!TryGetSkillDef(id, out def))
        {
            Debug.LogWarning($"[SkillManager] AddToLoadoutById failed. Unknown id={id}");
            return false;
        }
        return AddToLoadout(def);
    }

    public bool RemoveFromLoadoutById(string id, bool removeAll = false)
    {
        if (string.IsNullOrEmpty(id) || _loadout.Count == 0) return false;

        int removed = 0;
        if (removeAll) removed = _loadout.RemoveAll(s => s.Id == id);
        else
        {
            int idx = _loadout.FindIndex(s => s.Id == id);
            if (idx >= 0) { _loadout.RemoveAt(idx); removed = 1; }
        }

        if (removed > 0)
        {
            RaiseLoadoutChanged();
            return true;
        }
        return false;
    }

    public bool ToggleLoadout(LaboratoryData def)
    {
        int idx = _loadout.FindIndex(s => s.Id == def.Id);
        if (idx >= 0)
        {
            _loadout.RemoveAt(idx);
            RaiseLoadoutChanged();
            return true;
        }

        if (_loadout.Count >= _maxSlots)
        {
            Debug.LogWarning("[SkillManager] Loadout is full");
            return false;
        }

        _loadout.Add(new SelectedSkill(def));
        RaiseLoadoutChanged();
        return true;
    }

    public SelectedSkill GetSelectedSkillBySlotIndex(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _loadout.Count) throw new IndexOutOfRangeException();
        return _loadout[slotIndex];
    }

    // 공개 타게팅 스펙
    public bool TryGetTargetingSpec(SelectedSkill skill, out SkillTargetingSpec spec)
    {
        ISkillTargetingSpecProvider provider;
        if (_targetingProviders.TryGetValue(skill.Id, out provider))
        {
            spec = provider.GetSpec(skill);
            return true;
        }

        spec = new SkillTargetingSpec
        {
            Mode = TargetingMode.Point,
            HalfSizeCells = 0,
            RadiusWorld = 0f,
            ValidTargets = TargetKind.Both
        };
        return false;
    }

    // 공개 스킬 사용 API
    public void UseSkill(int slotIndex, GameObject explicitTarget = null)
    {
        if (PauseControl.IsPaused) return;

        if (slotIndex < 0 || slotIndex >= _loadout.Count)
        {
            Debug.LogWarning("[SkillManager] Invalid slot index");
            return;
        }

        SelectedSkill skill = _loadout[slotIndex];

        if (!CostManager.Instance || !CostManager.Instance.TrySpendSkill(skill.Cost))
        {
            Debug.LogWarning("[SkillManager] Not enough skill cost");
            return;
        }

        if (TryApplyIncomeReward(skill)) return;

        ApplySkillEffect(skill, explicitTarget);
    }

    public void UseSkillAreaRectWorld(int slotIndex, Vector3 centerWorld, int halfSizeCells, TargetKind targetKind)
    {
        if (PauseControl.IsPaused) return;

        if (slotIndex < 0 || slotIndex >= _loadout.Count)
        {
            Debug.LogWarning("[SkillManager] Invalid slot index");
            return;
        }

        SelectedSkill skill = _loadout[slotIndex];

        if (!CostManager.Instance || !CostManager.Instance.TrySpendSkill(skill.Cost))
        {
            Debug.LogWarning("[SkillManager] Not enough skill cost");
            return;
        }

        if (TryApplyIncomeReward(skill)) return;

        centerWorld.z = 0f;
        List<GameObject> targets = AcquireTargetsRectWorld(centerWorld, halfSizeCells, targetKind);
        ApplyEffectToTargets(skill, targets);
    }

    public void UseSkillAreaRadiusWorld(int slotIndex, Vector3 centerWorld, float radiusWorld, TargetKind targetKind)
    {
        if (PauseControl.IsPaused) return;

        if (slotIndex < 0 || slotIndex >= _loadout.Count)
        {
            Debug.LogWarning("[SkillManager] Invalid slot index");
            return;
        }

        SelectedSkill skill = _loadout[slotIndex];

        if (!CostManager.Instance || !CostManager.Instance.TrySpendSkill(skill.Cost))
        {
            Debug.LogWarning("[SkillManager] Not enough skill cost");
            return;
        }

        if (TryApplyIncomeReward(skill)) return;

        centerWorld.z = 0f;
        List<GameObject> targets = AcquireTargetsRadiusWorld(centerWorld, radiusWorld, targetKind);
        ApplyEffectToTargets(skill, targets);
    }

    // 공개 해금 API
    public bool UnlockSkillById(string id, bool raiseEvent = true)
    {
        if (string.IsNullOrEmpty(id)) return false;

        try
        {
            var gameData = DataManager.Instance != null ? DataManager.Instance.GameData : null;
            if (gameData == null) return false;

            if (gameData.UnlockedLaboratoryId == null)
                gameData.UnlockedLaboratoryId = new List<string>();

            if (gameData.UnlockedLaboratoryId.Contains(id))
                return false;

            gameData.UnlockedLaboratoryId.Add(id);
        }
        catch { return false; }

        RefreshUnlockedDatabaseFromDataManager(raiseEvent);
        return true;
    }

    public IReadOnlyList<string> GetUnlockedSkillIds()
    {
        return _unlockedIdsCache.AsReadOnly();
    }

    public void RefreshUnlockedDatabaseFromDataManager(bool raiseEvent = true)
    {
        List<LaboratoryData> src = null;
        try { src = DataManager.Instance.LaboratoryDatas; } catch { }

        _databaseList = FilterUnlocked(src);
        RebuildDefinitionIndex();
        RebuildUnlockedIdsCache();

        if (raiseEvent)
        {
            OnDatabaseChanged?.Invoke();
            OnUnlockedSkillIdsChanged?.Invoke(_unlockedIdsCache.AsReadOnly());
        }
    }

    // Unity 생명주기
    private void Awake()
    {
        if (_autoLoadFromDataManager)
        {
            List<LaboratoryData> src = null;
            try { src = DataManager.Instance.LaboratoryDatas; } catch { }
            _databaseList = FilterUnlocked(src);
        }
        else
        {
            _databaseList = FilterUnlocked(_databaseList);
        }

        RebuildDefinitionIndex();
        RebuildUnlockedIdsCache();

        OnDatabaseChanged?.Invoke();
        OnUnlockedSkillIdsChanged?.Invoke(_unlockedIdsCache.AsReadOnly());

        RegisterBuiltinSkills();
    }

    // 내부 헬퍼
    private void LoadDatabaseFromDataManager()
    {
        try
        {
            List<LaboratoryData> src = DataManager.Instance != null ? DataManager.Instance.LaboratoryDatas : null;
            if (src != null) _databaseList = new List<LaboratoryData>(src);
        }
        catch { }
    }

    private void RebuildDefinitionIndex()
    {
        _defsById.Clear();
        if (_databaseList == null) return;

        for (int i = 0; i < _databaseList.Count; i++)
        {
            LaboratoryData def = _databaseList[i];
            if (string.IsNullOrEmpty(def.Id))
            {
                Debug.LogWarning($"[SkillManager] LaboratoryData[{i}] has empty Id. Skipped.");
                continue;
            }
            _defsById[def.Id] = def;
        }
    }

    private void RebuildUnlockedIdsCache()
    {
        _unlockedIdsCache.Clear();
        if (_databaseList == null) return;
        for (int i = 0; i < _databaseList.Count; i++)
        {
            string id = _databaseList[i].Id;
            if (!string.IsNullOrEmpty(id)) _unlockedIdsCache.Add(id);
        }
    }

    private void RaiseLoadoutChanged()
    {
        OnLoadoutChanged?.Invoke();

        _loadoutIdsCache.Clear();
        for (int i = 0; i < _loadout.Count; i++)
            _loadoutIdsCache.Add(_loadout[i].Id);

        OnLoadoutIdsChanged?.Invoke(_loadoutIdsCache.AsReadOnly());
    }

    // 내부 레지스트리
    private void RegisterBuiltinSkills()
    {
        RangeHealSkill rangeHeal = new RangeHealSkill();
        RegisterTowerHandler(rangeHeal);
        RegisterTargetingProvider(rangeHeal);

        // _defaultMonsterHandler = new DefaultMonsterDamageSkill();

        RegisterIncomeHandler(new IncomeMoneyHandler());
        RegisterIncomeHandler(new IncomeSkillHandler());
    }

    private void RegisterTowerHandler(ITowerSkillHandler handler)
    {
        if (handler == null || string.IsNullOrEmpty(handler.Id)) return;
        _towerHandlers[handler.Id] = handler;
    }

    private void RegisterMonsterHandler(IMonsterSkillHandler handler)
    {
        if (handler == null || string.IsNullOrEmpty(handler.Id)) return;
        _monsterHandlers[handler.Id] = handler;
    }

    private void RegisterIncomeHandler(IIncomeSkillHandler handler)
    {
        if (handler == null) return;
        _incomeHandlers[handler.TargetType] = handler;
    }

    private void RegisterTargetingProvider(ISkillTargetingSpecProvider provider)
    {
        if (provider == null || string.IsNullOrEmpty(provider.Id)) return;
        _targetingProviders[provider.Id] = provider;
    }

    // 내부 디스패치
    private bool TryApplyIncomeReward(SelectedSkill selectedSkill)
    {
        IIncomeSkillHandler handler;
        if (_incomeHandlers.TryGetValue(selectedSkill.Effect.TargetType, out handler))
        {
            handler.Apply(selectedSkill);
            return true;
        }
        return false;
    }

    private void ApplySkillEffect(SelectedSkill selectedSkill, GameObject explicitTarget)
    {
        if (selectedSkill.Effect.TargetType != TargetType.Unit) return;
        if (explicitTarget == null) return;

        MapManager map = MapManager.Instance;
        if (map == null || !map.IsReady) return;

        if (explicitTarget.CompareTag(_monsterTag))
        {
            ApplyToMonsterObject(selectedSkill, explicitTarget);
            return;
        }

        Vector3Int cell = map.WorldToCell(explicitTarget.transform.position);
        GameObject towerAtCell;
        if (map.TryGetTowerAt(cell, out towerAtCell) && towerAtCell == explicitTarget)
        {
            ApplyToTowerObject(selectedSkill, explicitTarget);
        }
    }

    private void ApplyEffectToTargets(SelectedSkill selectedSkill, List<GameObject> targetObjects)
    {
        if (targetObjects == null || targetObjects.Count == 0) return;

        for (int i = 0; i < targetObjects.Count; i++)
        {
            GameObject obj = targetObjects[i];
            if (obj == null || !obj.activeInHierarchy) continue;

            if (obj.CompareTag(_monsterTag))
            {
                ApplyToMonsterObject(selectedSkill, obj);
            }
            else
            {
                ApplyToTowerObject(selectedSkill, obj);
            }
        }
    }

    private void ApplyToTowerObject(SelectedSkill selectedSkill, GameObject towerObject)
    {
        ITowerSkillHandler handler;
        if (_towerHandlers.TryGetValue(selectedSkill.Id, out handler))
        {
            handler.Apply(towerObject, selectedSkill);
        }
    }

    private void ApplyToMonsterObject(SelectedSkill selectedSkill, GameObject monsterObject)
    {
        IMonsterSkillHandler handler;
        if (_monsterHandlers.TryGetValue(selectedSkill.Id, out handler))
        {
            handler.Apply(monsterObject, selectedSkill);
            return;
        }

        if (_defaultMonsterHandler != null)
        {
            _defaultMonsterHandler.Apply(monsterObject, selectedSkill);
        }
    }

    // 내부 타겟 수집
    private List<GameObject> AcquireTargetsRectWorld(Vector3 worldCenter, int halfSizeCells, TargetKind targetKind)
    {
        MapManager map = MapManager.Instance;
        if (map == null || !map.IsReady) return new List<GameObject>();
        worldCenter.z = 0f;
        Vector3Int centerCell = map.WorldToCell(worldCenter);
        return AcquireTargetsRectCells(centerCell, halfSizeCells, targetKind);
    }

    private List<GameObject> AcquireTargetsRectCells(Vector3Int centerCell, int halfSizeCells, TargetKind targetKind)
    {
        MapManager map = MapManager.Instance;
        List<GameObject> results = new List<GameObject>(32);
        if (map == null || !map.IsReady) return results;

        HashSet<GameObject> uniq = new HashSet<GameObject>();

        BoundsInt b = map.GetNavBounds();
        int minX = Mathf.Max(b.xMin, centerCell.x - halfSizeCells);
        int maxX = Mathf.Min(b.xMax - 1, centerCell.x + halfSizeCells);
        int minY = Mathf.Max(b.yMin, centerCell.y - halfSizeCells);
        int maxY = Mathf.Min(b.yMax - 1, centerCell.y + halfSizeCells);

        if ((targetKind & TargetKind.Towers) != 0)
        {
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Vector3Int cell = new Vector3Int(x, y, 0);
                    GameObject tower;
                    if (map.TryGetTowerAt(cell, out tower) && tower != null && tower.activeInHierarchy)
                    {
                        if (uniq.Add(tower)) results.Add(tower);
                    }
                }
            }
        }

        if ((targetKind & TargetKind.Monsters) != 0)
        {
            GameObject[] monsters = GameObject.FindGameObjectsWithTag(_monsterTag);
            for (int i = 0; i < monsters.Length; i++)
            {
                GameObject m = monsters[i];
                if (m == null || !m.activeInHierarchy) continue;
                Vector3Int c = map.WorldToCell(m.transform.position);
                if (c.x < minX || c.x > maxX || c.y < minY || c.y > maxY) continue;
                if (uniq.Add(m)) results.Add(m);
            }
        }

        return results;
    }

    private List<GameObject> AcquireTargetsRadiusWorld(Vector3 worldCenter, float radiusWorld, TargetKind targetKind)
    {
        MapManager map = MapManager.Instance;
        List<GameObject> results = new List<GameObject>(32);
        if (map == null || !map.IsReady) return results;

        worldCenter.z = 0f;
        float r2 = radiusWorld * radiusWorld;
        HashSet<GameObject> uniq = new HashSet<GameObject>();

        Vector3Int originCell, sizeCells;
        Vector3 cellSize;
        map.GetNavFrame(out originCell, out sizeCells, out cellSize);

        float cellStep = Mathf.Max(Mathf.Abs(cellSize.x), Mathf.Abs(cellSize.y));
        int half = Mathf.Max(0, Mathf.CeilToInt(radiusWorld / Mathf.Max(cellStep, 0.0001f)));

        Vector3Int centerCell = map.WorldToCell(worldCenter);

        if ((targetKind & TargetKind.Towers) != 0)
        {
            List<GameObject> towerCandidates = AcquireTargetsRectCells(centerCell, half, TargetKind.Towers);
            for (int i = 0; i < towerCandidates.Count; i++)
            {
                GameObject t = towerCandidates[i];
                if (t == null || !t.activeInHierarchy) continue;
                if ((t.transform.position - worldCenter).sqrMagnitude <= r2 && uniq.Add(t))
                    results.Add(t);
            }
        }

        if ((targetKind & TargetKind.Monsters) != 0)
        {
            GameObject[] monsters = GameObject.FindGameObjectsWithTag(_monsterTag);
            for (int i = 0; i < monsters.Length; i++)
            {
                GameObject m = monsters[i];
                if (m == null || !m.activeInHierarchy) continue;
                if ((m.transform.position - worldCenter).sqrMagnitude <= r2 && uniq.Add(m))
                    results.Add(m);
            }
        }

        return results;
    }

    // 내부 해금 유틸
    private static HashSet<string> GetUnlockedIdSet()
    {
        HashSet<string> set = new HashSet<string>();
        try
        {
            if (DataManager.Instance != null && DataManager.Instance.GameData != null)
            {
                List<string> ids = DataManager.Instance.GameData.UnlockedLaboratoryId;
                if (ids != null)
                {
                    for (int i = 0; i < ids.Count; i++)
                    {
                        string id = ids[i];
                        if (!string.IsNullOrEmpty(id)) set.Add(id);
                    }
                }
            }
        }
        catch { }
        return set;
    }

    private static List<LaboratoryData> FilterUnlocked(List<LaboratoryData> source)
    {
        List<LaboratoryData> result = new List<LaboratoryData>();
        if (source == null || source.Count == 0) return result;

        HashSet<string> unlocked = GetUnlockedIdSet();
        if (unlocked.Count == 0) return result;

        for (int i = 0; i < source.Count; i++)
        {
            LaboratoryData d = source[i];
            if (!string.IsNullOrEmpty(d.Id) && unlocked.Contains(d.Id))
                result.Add(d);
        }
        return result;
    }
}
