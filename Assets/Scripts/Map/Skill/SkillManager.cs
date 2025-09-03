using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public partial class SkillManager : MonoSingleton<SkillManager>
{
    // ===== 데이터/선택 =====
    [Header("데이터/선택")]
    [SerializeField] private LaboratoryData database; // 필요 시 사용
    [SerializeField] private int maxSlots = 3;
    [SerializeField] public List<SelectedSkill> loadout = new List<SelectedSkill>();

    [Header("설정")]
    [SerializeField] private Transform unitRoot;      // 선택: 유닛 부모(없어도 무방)
    [SerializeField] private string monsterTag = "Monster";

    // 핸들러 레지스트리
    private Dictionary<string, ITowerSkillHandler> towerHandlers = new Dictionary<string, ITowerSkillHandler>();
    private Dictionary<string, IMonsterSkillHandler> monsterHandlers = new Dictionary<string, IMonsterSkillHandler>();
    private Dictionary<TargetType, IIncomeSkillHandler> incomeHandlers = new Dictionary<TargetType, IIncomeSkillHandler>();
    private Dictionary<string, ISkillTargetingSpecProvider> targetingProviders = new Dictionary<string, ISkillTargetingSpecProvider>();

    // 선택: 몬스터 기본 처리자(등록 없음 시)
    private IMonsterSkillHandler defaultMonsterHandler;

    public event Action OnLoadoutChanged;

    // ===== SelectedSkill (데이터 최소 필드만) =====
    public struct SelectedSkill
    {
        public string Id;
        public int Cost;
        public float Value;
        public ValueType ValueType;   // Add만 사용
        public TargetType TargetType; // Unit / IncomeMoney / IncomeSkill

        public SelectedSkill(LaboratoryData def)
        {
            Id = def.Id;
            Cost = def.Cost;
            Value = def.Value;
            ValueType = def.ValueType;
            TargetType = def.TargetType;
        }
    }

    // ===== 로드/등록 =====
    private void Awake()
    {
        RegisterBuiltinSkills();
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
        towerHandlers[handler.Id] = handler;
    }

    private void RegisterMonsterHandler(IMonsterSkillHandler handler)
    {
        if (handler == null || string.IsNullOrEmpty(handler.Id)) return;
        monsterHandlers[handler.Id] = handler;
    }

    private void RegisterIncomeHandler(IIncomeSkillHandler handler)
    {
        if (handler == null) return;
        incomeHandlers[handler.TargetType] = handler;
    }

    private void RegisterTargetingProvider(ISkillTargetingSpecProvider provider)
    {
        if (provider == null || string.IsNullOrEmpty(provider.Id)) return;
        targetingProviders[provider.Id] = provider;
    }

    // ===== 외부 API =====
    public bool AddToLoadout(LaboratoryData def)
    {
        if (loadout.Count >= maxSlots) { Debug.LogWarning("[SkillManager] 로드아웃 가득 참"); return false; }
        SelectedSkill picked = new SelectedSkill(def);
        loadout.Add(picked);
        Action ev = OnLoadoutChanged;
        if (ev != null) ev.Invoke();
        return true;
    }

    public SelectedSkill GetSelectedSkillBySlotIndex(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= loadout.Count) throw new IndexOutOfRangeException();
        return loadout[slotIndex];
    }

    public bool TryGetTargetingSpec(SelectedSkill skill, out SkillTargetingSpec spec)
    {
        ISkillTargetingSpecProvider provider;
        if (targetingProviders.TryGetValue(skill.Id, out provider))
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
        if (slotIndex < 0 || slotIndex >= loadout.Count) { Debug.LogWarning("[SkillManager] 잘못된 슬롯 인덱스"); return; }
        SelectedSkill skill = loadout[slotIndex];

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
        if (slotIndex < 0 || slotIndex >= loadout.Count) { Debug.LogWarning("[SkillManager] 잘못된 슬롯 인덱스"); return; }
        SelectedSkill skill = loadout[slotIndex];

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
        if (slotIndex < 0 || slotIndex >= loadout.Count) { Debug.LogWarning("[SkillManager] 잘못된 슬롯 인덱스"); return; }
        SelectedSkill skill = loadout[slotIndex];

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
        if (selectedSkill.TargetType != TargetType.Unit) return;

        if (explicitTarget == null) return;

        MapManager mapManager = MapManager.Instance;
        if (mapManager == null || !mapManager.IsReady) return;

        if (explicitTarget.CompareTag(monsterTag))
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

            if (targetObject.CompareTag(monsterTag))
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
        if (towerHandlers.TryGetValue(selectedSkill.Id, out handler))
        {
            handler.Apply(towerObject, selectedSkill);
        }
    }

    private void ApplyToMonsterObject(SelectedSkill selectedSkill, GameObject monsterObject)
    {
        IMonsterSkillHandler handler;
        if (monsterHandlers.TryGetValue(selectedSkill.Id, out handler))
        {
            handler.Apply(monsterObject, selectedSkill);
            return;
        }

        if (defaultMonsterHandler != null)
        {
            defaultMonsterHandler.Apply(monsterObject, selectedSkill);
        }
    }

    private bool TryApplyIncomeReward(SelectedSkill selectedSkill)
    {
        IIncomeSkillHandler handler;
        if (incomeHandlers.TryGetValue(selectedSkill.TargetType, out handler))
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
            GameObject[] monsterObjects = GameObject.FindGameObjectsWithTag(monsterTag);
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
            GameObject[] monsterObjects = GameObject.FindGameObjectsWithTag(monsterTag);
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
