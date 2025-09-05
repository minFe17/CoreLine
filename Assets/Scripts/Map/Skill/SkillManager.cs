using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public partial class SkillManager : MonoSingleton<SkillManager>
{
    // ===== 데이터/선택 =====
    [Header("데이터/선택")]
    [SerializeField] private List<LaboratoryData> _databaseList = new List<LaboratoryData>(); // 필요 시 사용
    [SerializeField] private bool _autoLoadFromDataManager = true;
    [SerializeField] private int _maxSlots = 3;
    [SerializeField] public List<SelectedSkill> _loadout = new List<SelectedSkill>();

    [Header("설정")]
    [SerializeField] private Transform _unitRoot;      // 선택: 유닛 부모(없어도 무방)
    [SerializeField] private string _monsterTag = "Monster";

    private readonly Dictionary<string, LaboratoryData> _defsById = new Dictionary<string, LaboratoryData>();

    // 핸들러 레지스트리
    private Dictionary<string, ITowerSkillHandler> _towerHandlers = new Dictionary<string, ITowerSkillHandler>();
    private Dictionary<string, IMonsterSkillHandler> _monsterHandlers = new Dictionary<string, IMonsterSkillHandler>();
    private Dictionary<TargetType, IIncomeSkillHandler> _incomeHandlers = new Dictionary<TargetType, IIncomeSkillHandler>();
    private Dictionary<string, ISkillTargetingSpecProvider> _targetingProviders = new Dictionary<string, ISkillTargetingSpecProvider>();

    // 선택: 몬스터 기본 처리자(등록 없음 시)
    private IMonsterSkillHandler _defaultMonsterHandler;

    public event Action OnLoadoutChanged;
    public event Action OnDatabaseChanged;

    // ===== SelectedSkill (데이터 최소 필드만) =====
    public struct SelectedSkill
    {
        public string Id;
        public int Cost;
        public Effect Effect;   // Value / ValueType / TargetType / TargetStatus 전부 포함

        public SelectedSkill(LaboratoryData def)
        {
            Id = def.Id;
            Cost = def.Cost;
            Effect = def.Effect;  // 통째로 복사
        }
    }


    // ===== 로드/등록 =====
    private void Awake()
    {
        InitDatabase();
        RegisterBuiltinSkills();
    }
    // DB 초기화 (DataManager에서 자동 로드  인덱스 구축)
    private void InitDatabase()
    {
        if (_autoLoadFromDataManager && (_databaseList == null || _databaseList.Count == 0))
        {
            List<LaboratoryData> src = null;
            try { src = DataManager.Instance.LaboratoryDatas; } catch { /* 무시 */ }
            if (src != null) _databaseList = new List<LaboratoryData>(src);
        }

        RebuildDefinitionIndex();

        Action ev = OnDatabaseChanged;
        if (ev != null) ev.Invoke();
    }

    // DB 인덱스 재구축
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

    // ===== 로비 UI/외부에서 쓰는 공개 API =====

    /// DB 전체(읽기전용) 반환  로비 UI에서 그리드 생성에 사용
    public IReadOnlyList<LaboratoryData> GetAllSkillDefs()
    {
        return _databaseList ?? (IReadOnlyList<LaboratoryData>)Array.Empty<LaboratoryData>();
    }

    /// 타입별 필터 (공격/방어/유틸리티 등 카테고리 탭에 사용)
    public List<LaboratoryData> GetSkillDefsByType(LaboratoryType type)
    {
        List<LaboratoryData> list = new List<LaboratoryData>();
        if (_databaseList == null) return list;

        for (int i = 0; i < _databaseList.Count; i++)
        {
            LaboratoryData def = _databaseList[i];

            // 1) 평평한 구조인 경우: def.Type
            // if (def.Type == type) list.Add(def);

            // 2) 상위 스키마에서 LaboratoryType 필드명이 다르면 여기에 맞춰 수정
            if (def.LaboratoryType == type) list.Add(def);
        }
        return list;
    }

    /// Id로 정의 얻기 (버튼 클릭 등에서)
    public bool TryGetSkillDef(string id, out LaboratoryData def)
    {
        return _defsById.TryGetValue(id, out def);
    }

    /// 외부에서 DB 교체/세팅  로비에서 동적 소팅/필터 결과 반영 등
    public void SetDatabase(List<LaboratoryData> newList, bool rebuildIndex = true, bool raiseEvent = true)
    {
        _databaseList = newList ?? new List<LaboratoryData>();
        if (rebuildIndex) RebuildDefinitionIndex();
        if (raiseEvent && OnDatabaseChanged != null) OnDatabaseChanged.Invoke();
    }

    /// 로드아웃에 Id로 추가 (UI 버튼에서 바로 연결하기 쉬움)
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
        if (removeAll)
        {
            removed = _loadout.RemoveAll(s => s.Id == id);
        }
        else
        {
            int idx = _loadout.FindIndex(s => s.Id == id);
            if (idx >= 0) { _loadout.RemoveAt(idx); removed = 1; }
        }

        if (removed > 0)
        {
            Action ev = OnLoadoutChanged;
            if (ev != null) ev.Invoke();
            return true;
        }
        return false;
    }
    public bool ToggleLoadout(LaboratoryData def)
    {
        // 이미 있으면 해제
        int idx = _loadout.FindIndex(s => s.Id == def.Id);
        if (idx >= 0)
        {
            _loadout.RemoveAt(idx);
            OnLoadoutChanged?.Invoke();
            return true;
        }

        // 없으면 장착(슬롯 체크)
        if (_loadout.Count >= _maxSlots)
        {
            Debug.LogWarning("[SkillManager] 로드아웃 가득 참");
            return false;
        }

        _loadout.Add(new SelectedSkill(def));
        OnLoadoutChanged?.Invoke();
        return true;
    }
    private void RegisterBuiltinSkills()
    {
        // RangeHeal: 효과 + 타게팅 제공을 하나의 인스턴스로
        RangeHealSkill rangeHeal = new RangeHealSkill();
        RegisterTowerHandler(rangeHeal);
        RegisterTargetingProvider(rangeHeal);

        // 몬스터용 스킬 (기본 처리자)
        //defaultMonsterHandler = new DefaultMonsterDamageSkill();
        // 예: 특수 몬스터 스킬 추가하려면 아래처럼 등록
        // RegisterMonsterHandler(new RangeNukeSkill());

        // 인컴(보상) 스킬
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

    // ===== 외부 API =====
    public bool AddToLoadout(LaboratoryData def)
    {
        if (_loadout.Count >= _maxSlots) { Debug.LogWarning("[SkillManager] 로드아웃 가득 참"); return false; }
        SelectedSkill picked = new SelectedSkill(def);
        _loadout.Add(picked);
        Action ev = OnLoadoutChanged;
        if (ev != null) ev.Invoke();
        return true;
    }
    public SelectedSkill GetSelectedSkillBySlotIndex(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _loadout.Count) throw new IndexOutOfRangeException();
        return _loadout[slotIndex];
    }

    public bool TryGetTargetingSpec(SelectedSkill skill, out SkillTargetingSpec spec)
    {
        ISkillTargetingSpecProvider provider;
        if (_targetingProviders.TryGetValue(skill.Id, out provider))
        {
            spec = provider.GetSpec(skill);
            return true;
        }
        // 기본값: 포인트 타겟
        spec = new SkillTargetingSpec
        {
            Mode = TargetingMode.Point,
            HalfSizeCells = 0,
            RadiusWorld = 0f,
            ValidTargets = TargetKind.Both
        };
        return false;
    }

    // ===== 스킬 사용(단일/범위) =====
    public void UseSkill(int slotIndex, GameObject explicitTarget = null)
    {
        if (PauseControl.IsPaused) return;

        if (slotIndex < 0 || slotIndex >= _loadout.Count) { Debug.LogWarning("[SkillManager] 잘못된 슬롯 인덱스"); return; }
        SelectedSkill skill = _loadout[slotIndex];

        // 비용: 스킬 지갑
        if (!CostManager.Instance || !CostManager.Instance.TrySpendSkill(skill.Cost))
        {
            Debug.LogWarning("[SkillManager] 스킬 코스트 부족");
            return;
        }

        // 인컴(보상) 타입이면 즉시 지급
        if (TryApplyIncomeReward(skill)) return;

        // 실제 효과 적용
        ApplySkillEffect(skill, explicitTarget);
    }

    public void UseSkillAreaRectWorld(int slotIndex, Vector3 centerWorld, int halfSizeCells, TargetKind targetKind)
    {
        if (PauseControl.IsPaused) return;

        if (slotIndex < 0 || slotIndex >= _loadout.Count) { Debug.LogWarning("[SkillManager] 잘못된 슬롯 인덱스"); return; }
        SelectedSkill skill = _loadout[slotIndex];

        if (!CostManager.Instance || !CostManager.Instance.TrySpendSkill(skill.Cost))
        {
            Debug.LogWarning("[SkillManager] 스킬 코스트 부족");
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

        if (slotIndex < 0 || slotIndex >= _loadout.Count) { Debug.LogWarning("[SkillManager] 잘못된 슬롯 인덱스"); return; }
        SelectedSkill skill = _loadout[slotIndex];

        if (!CostManager.Instance || !CostManager.Instance.TrySpendSkill(skill.Cost))
        {
            Debug.LogWarning("[SkillManager] 스킬 코스트 부족");
            return;
        }

        if (TryApplyIncomeReward(skill)) return;

        centerWorld.z = 0f;
        List<GameObject> targets = AcquireTargetsRadiusWorld(centerWorld, radiusWorld, targetKind);
        ApplyEffectToTargets(skill, targets);
    }

    // ===== 디스패치 =====
    private void ApplySkillEffect(SelectedSkill selectedSkill, GameObject explicitTarget)
    {
        if (selectedSkill.Effect.TargetType != TargetType.Unit) return;

        if (explicitTarget == null) return;

        MapManager mapManager = MapManager.Instance;
        if (mapManager == null || !mapManager.IsReady) return;

        if (explicitTarget.CompareTag(_monsterTag))
        {
            ApplyToMonsterObject(selectedSkill, explicitTarget);
            return;
        }

        Vector3Int cell = mapManager.WorldToCell(explicitTarget.transform.position);
        GameObject towerAtCell;
        if (mapManager.TryGetTowerAt(cell, out towerAtCell) && towerAtCell == explicitTarget)
        {
            ApplyToTowerObject(selectedSkill, explicitTarget);
        }
    }

    private void ApplyEffectToTargets(SelectedSkill selectedSkill, List<GameObject> targetObjects)
    {
        if (targetObjects == null || targetObjects.Count == 0) return;

        for (int i = 0; i < targetObjects.Count; i++)
        {
            GameObject targetObject = targetObjects[i];
            if (targetObject == null || !targetObject.activeInHierarchy) continue;

            if (targetObject.CompareTag(_monsterTag))
            {
                ApplyToMonsterObject(selectedSkill, targetObject);
            }
            else
            {
                ApplyToTowerObject(selectedSkill, targetObject);
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

    // ===== 타겟 수집 유틸 (당신 프로젝트의 기존 버전 재사용) =====

    [System.Flags]
    public enum TargetKind
    {
        Towers = 1 << 0,
        Monsters = 1 << 1,
        Both = Towers | Monsters
    }

    private List<GameObject> AcquireTargetsRectWorld(Vector3 worldCenter, int halfSizeCells, TargetKind targetKind)
    {
        MapManager mapManager = MapManager.Instance;
        if (mapManager == null || !mapManager.IsReady) return new List<GameObject>();
        Vector3 worldCenter2D = worldCenter; worldCenter2D.z = 0f;
        Vector3Int centerCell = mapManager.WorldToCell(worldCenter2D);
        return AcquireTargetsRectCells(centerCell, halfSizeCells, targetKind);
    }

    private List<GameObject> AcquireTargetsRectCells(Vector3Int centerCell, int halfSizeCells, TargetKind targetKind)
    {
        MapManager mapManager = MapManager.Instance;
        List<GameObject> resultObjects = new List<GameObject>(32);
        if (mapManager == null || !mapManager.IsReady) return resultObjects;

        HashSet<GameObject> uniqueObjects = new HashSet<GameObject>();

        BoundsInt navigationBounds = mapManager.GetNavBounds();
        int minCellX = Mathf.Max(navigationBounds.xMin, centerCell.x - halfSizeCells);
        int maxCellX = Mathf.Min(navigationBounds.xMax - 1, centerCell.x + halfSizeCells);
        int minCellY = Mathf.Max(navigationBounds.yMin, centerCell.y - halfSizeCells);
        int maxCellY = Mathf.Min(navigationBounds.yMax - 1, centerCell.y + halfSizeCells);

        if ((targetKind & TargetKind.Towers) != 0)
        {
            for (int cellY = minCellY; cellY <= maxCellY; cellY++)
            {
                for (int cellX = minCellX; cellX <= maxCellX; cellX++)
                {
                    Vector3Int cell = new Vector3Int(cellX, cellY, 0);
                    GameObject towerObject;
                    if (mapManager.TryGetTowerAt(cell, out towerObject) && towerObject != null && towerObject.activeInHierarchy)
                    {
                        if (uniqueObjects.Add(towerObject)) resultObjects.Add(towerObject);
                    }
                }
            }
        }

        if ((targetKind & TargetKind.Monsters) != 0)
        {
            GameObject[] monsterObjects = GameObject.FindGameObjectsWithTag(_monsterTag);
            foreach (GameObject monsterObject in monsterObjects)
            {
                if (monsterObject == null || !monsterObject.activeInHierarchy) continue;
                Vector3Int monsterCell = mapManager.WorldToCell(monsterObject.transform.position);
                if (monsterCell.x < minCellX || monsterCell.x > maxCellX || monsterCell.y < minCellY || monsterCell.y > maxCellY) continue;
                if (uniqueObjects.Add(monsterObject)) resultObjects.Add(monsterObject);
            }
        }

        return resultObjects;
    }

    private List<GameObject> AcquireTargetsRadiusWorld(Vector3 worldCenter, float radiusWorld, TargetKind targetKind)
    {
        MapManager mapManager = MapManager.Instance;
        List<GameObject> resultObjects = new List<GameObject>(32);
        if (mapManager == null || !mapManager.IsReady) return resultObjects;

        Vector3 worldCenter2D = worldCenter; worldCenter2D.z = 0f;
        float radiusSquared = radiusWorld * radiusWorld;
        HashSet<GameObject> uniqueObjects = new HashSet<GameObject>();

        Vector3Int originCell; Vector3Int sizeCells; Vector3 cellSize;
        mapManager.GetNavFrame(out originCell, out sizeCells, out cellSize);

        float cellStep = Mathf.Max(Mathf.Abs(cellSize.x), Mathf.Abs(cellSize.y));
        int halfSizeCells = Mathf.Max(0, Mathf.CeilToInt(radiusWorld / Mathf.Max(cellStep, 0.0001f)));

        Vector3Int centerCell = mapManager.WorldToCell(worldCenter2D);

        if ((targetKind & TargetKind.Towers) != 0)
        {
            List<GameObject> towerCandidates = AcquireTargetsRectCells(centerCell, halfSizeCells, TargetKind.Towers);
            for (int i = 0; i < towerCandidates.Count; i++)
            {
                GameObject towerObject = towerCandidates[i];
                if (towerObject == null || !towerObject.activeInHierarchy) continue;
                float distanceSquared = (towerObject.transform.position - worldCenter2D).sqrMagnitude;
                if (distanceSquared <= radiusSquared && uniqueObjects.Add(towerObject))
                    resultObjects.Add(towerObject);
            }
        }

        if ((targetKind & TargetKind.Monsters) != 0)
        {
            GameObject[] monsterObjects = GameObject.FindGameObjectsWithTag(_monsterTag);
            for (int i = 0; i < monsterObjects.Length; i++)
            {
                GameObject monsterObject = monsterObjects[i];
                if (monsterObject == null || !monsterObject.activeInHierarchy) continue;
                float distanceSquared = (monsterObject.transform.position - worldCenter2D).sqrMagnitude;
                if (distanceSquared <= radiusSquared && uniqueObjects.Add(monsterObject))
                    resultObjects.Add(monsterObject);
            }
        }

        return resultObjects;
    }
}
