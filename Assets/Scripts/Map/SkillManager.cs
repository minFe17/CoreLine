using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Utils;

public class SkillManager : MonoSingleton<SkillManager>
{
    public struct SelectedSkill
    {
        public string Id;
        public int Cost;
        public float Value;
        public ValueType ValueType;
        public TargetType TargetType;

        public SelectedSkill(LaboratoryData def)
        {
            Id = def.Id;
            Cost = def.Cost;
            Value = def.Value;
            ValueType = def.ValueType;
            TargetType = def.TargetType;
        }
    }

    [System.Flags]
    public enum TargetKind
    {
        Towers = 1 << 0,
        Monsters = 1 << 1,
        Both = Towers | Monsters
    }
    [Header("데이터/선택")]
    [SerializeField] LaboratoryData database;
    [SerializeField] int maxSlots = 3;
    [SerializeField] public List<SelectedSkill> loadout = new(); // 선택된 스킬들

    [Header("효과 기본 지속시간(초) - 0이면 영구적")]
    [SerializeField] float defaultDurationSec = 10f;

    [SerializeField] private Transform unitRoot;      // 유닛들이 모여있는 부모. 없으면 씬 전체에서 탐색
    [SerializeField] private string monsterTag = "Monster"; // 몬스터 태그

    public event Action OnLoadoutChanged;

    // 선택창에서 호출 스킬 1개를 로드아웃에 추가
    public bool AddToLoadout(LaboratoryData def)
    {
        //if (def == null) return false;
        if (loadout.Count >= maxSlots) { Debug.LogWarning("[SkillManager] 로드아웃이 가득 찼습니다."); return false; }

        SelectedSkill picked = new SelectedSkill(def);
        loadout.Add(picked);
        OnLoadoutChanged?.Invoke();
        Debug.Log($"[SkillManager] Added {def.Id} (Cost {def.Cost})");
        return true;
    }

    // 버튼으로 발동 slotIndex는 0..N-1
    public void UseSkill(int slotIndex, GameObject explicitTarget = null)
    {
        if (slotIndex < 0 || slotIndex >= loadout.Count) { Debug.LogWarning("[SkillManager] 잘못된 슬롯 인덱스"); return; }
        SelectedSkill skill = loadout[slotIndex];

        // 스킬 지갑에서 비용 차감
        if (!CostManager.Instance || !CostManager.Instance.TrySpendSkill(skill.Cost))
        {
            Debug.LogWarning("[SkillManager] 골드가 부족합니다.");
            return;
        }

        //인컴 타입이면 즉시 지급 후 종료
        if (TryApplyIncomeReward(skill))
        {
            Debug.Log($"[SkillManager] Income: {skill.TargetType} +{skill.Value}");
            return;
        }

        // 그 외 실제 효과 적용
        ApplySkillEffect(skill, explicitTarget);
        Debug.Log($"[SkillManager] Used {skill.Id}. Spent {skill.Cost} gold.");
    }

    // =======================================================
    // 공통 실행 진입점
    // =======================================================

    // 버튼으로 발동 기존 explicitTarget이 있을 때 단일 적용
    private void ApplySkillEffect(SelectedSkill selectedSkill, GameObject explicitTarget)
    {
        if (explicitTarget == null)
        {
            // 단일 타겟이 없으면 아무 것도 하지 않음
            // 범위형은 아래 UseSkill_AreaRectWorld / UseSkill_AreaRadiusWorld로 호출하세요
            return;
        }

        MapManager mapManager = MapManager.Instance;
        if (mapManager == null || !mapManager.IsReady) return;

        // 몬스터 판정: 태그로
        if (explicitTarget.CompareTag(monsterTag))
        {
            ApplyToMonsterObject(selectedSkill, explicitTarget);
            return;
        }

        // 타워 판정 그 셀에 등록된 타워인지 확인
        Vector3Int cell = mapManager.WorldToCell(explicitTarget.transform.position);
        GameObject towerAtCell;
        if (mapManager.TryGetTowerAt(cell, out towerAtCell) && towerAtCell == explicitTarget)
        {
            ApplyToTowerObject(selectedSkill, explicitTarget);
        }
        // 둘 다 아니면 무시 (빈 셀/기타 오브젝트)
    }

    // =======================================================
    // 범위형 스킬: 사각(셀) / 원형(월드 반경)
    //  - 비용 차감 + 타겟 수집 + 분배(타워=힐, 몬스터=데미지)
    // =======================================================

    // 사각(셀) 범위  centerWorld 기준 halfSizeCells (1 = 3x3)
    public void UseSkillAreaRectWorld(int slotIndex, Vector3 centerWorld, int halfSizeCells, TargetKind targetKind = TargetKind.Both)
    {
        if (slotIndex < 0 || slotIndex >= loadout.Count) { Debug.LogWarning("[SkillManager] 잘못된 슬롯 인덱스"); return; }
        SelectedSkill selectedSkill = loadout[slotIndex];

        if (!CostManager.Instance || !CostManager.Instance.TrySpendSkill(selectedSkill.Cost))
        {
            Debug.LogWarning("[SkillManager] 골드가 부족합니다.");
            return;
        }

        //  인컴 타입이면 즉시 지급 후 종료
        if (TryApplyIncomeReward(selectedSkill))
        {
            Debug.Log($"[SkillManager] Income(Rect): {selectedSkill.TargetType} +{selectedSkill.Value}");
            return;
        }

        centerWorld.z = 0f;
        List<GameObject> targets = AcquireTargetsRectWorld(centerWorld, halfSizeCells, targetKind);
        ApplyEffectToTargets(selectedSkill, targets);
        Debug.Log($"[SkillManager] AreaRect ({(halfSizeCells * 2 + 1)}x{(halfSizeCells * 2 + 1)}) {selectedSkill.Id} → {targets.Count} target(s)");
    }

    public void UseSkillAreaRadiusWorld(int slotIndex, Vector3 centerWorld, float radiusWorld, TargetKind targetKind = TargetKind.Both)
    {
        if (slotIndex < 0 || slotIndex >= loadout.Count) { Debug.LogWarning("[SkillManager] 잘못된 슬롯 인덱스"); return; }
        SelectedSkill selectedSkill = loadout[slotIndex];

        if (!CostManager.Instance || !CostManager.Instance.TrySpendSkill(selectedSkill.Cost))
        {
            Debug.LogWarning("[SkillManager] 골드가 부족합니다.");
            return;
        }

        //  인컴 타입이면 즉시 지급 후 종료
        if (TryApplyIncomeReward(selectedSkill))
        {
            Debug.Log($"[SkillManager] Income(Radius): {selectedSkill.TargetType} +{selectedSkill.Value}");
            return;
        }

        centerWorld.z = 0f;
        List<GameObject> targets = AcquireTargetsRadiusWorld(centerWorld, radiusWorld, targetKind);
        ApplyEffectToTargets(selectedSkill, targets);
        Debug.Log($"[SkillManager] AreaRadius (r={radiusWorld:F2}) {selectedSkill.Id} → {targets.Count} target(s)");
    }

    // 타겟 분배 몬스터/타워 구분해서 각 처리로 보냄
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
                // 타워로 간주 (Acquire*에서 가져온 "Towers"는 MapManager 등록 타워만 들어옴)
                ApplyToTowerObject(selectedSkill, targetObject);
            }
        }
    }
    private bool TryApplyIncomeReward(SelectedSkill selectedSkill)
    {
        if (CostManager.Instance == null) return false;

        //if (selectedSkill.TargetType == TargetType.IncomeMoney)
        //{
        //    // 유닛 지갑으로 지급
        //    CostManager.Instance.AddUnit(selectedSkill.Value);  // ValueType은 Add만 사용
        //    return true;
        //}
        //
        //if (selectedSkill.TargetType == TargetType.IncomeSkill)
        //{
        //    // 스킬 지갑으로 지급
        //    CostManager.Instance.AddSkill(selectedSkill.Value);
        //    return true;
        //}

        return false;
    }
    // === 타워 힐만 적용 ===
    private void ApplyToTowerObject(SelectedSkill selectedSkill, GameObject towerObject)
    {
        // 여기서 실제 힐을 호출하세요
        // 예 1) TowerHealth 컴포넌트가 있을 때
        // TowerHealth health = towerObject.GetComponent<TowerHealth>();
        // if (health != null) health.Heal(selectedSkill.Value);

        // 예 2) 범용 HP
        // UnitHealth unitHealth = towerObject.GetComponent<UnitHealth>();
        // if (unitHealth != null) unitHealth.Heal(selectedSkill.Value);

        // 예 3) 메시지 방식
        // towerObject.SendMessage("OnSkillHeal", selectedSkill.Value, SendMessageOptions.DontRequireReceiver);
    }

    // === 몬스터 데미지만 적용 ===
    private void ApplyToMonsterObject(SelectedSkill selectedSkill, GameObject monsterObject)
    {
        // 여기서 실제 데미지를 호출하세요
        // 예 1) MonsterHealth 컴포넌트가 있을 때
        // MonsterHealth mh = monsterObject.GetComponent<MonsterHealth>();
        // if (mh != null) mh.TakeDamage(selectedSkill.Value);

        // 예 2) 범용 HP
        // UnitHealth hp = monsterObject.GetComponent<UnitHealth>();
        // if (hp != null) hp.TakeDamage(selectedSkill.Value);

        // 예 3) 메시지 방식
        // monsterObject.SendMessage("OnSkillDamage", selectedSkill.Value, SendMessageOptions.DontRequireReceiver);
    }

    // ───────────────────────────────────────────────────────────────────────
    // 사각(셀) 범위 centerCell 기준 (halfSizeCells*2+1)×(halfSizeCells*2+1)
    // ───────────────────────────────────────────────────────────────────────
    private List<GameObject> AcquireTargetsRectCells(Vector3Int centerCell, int halfSizeCells, TargetKind targetKind = TargetKind.Towers)
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

    // 월드 좌표 중심에서 셀 변환 후 사각(셀) 범위
    private List<GameObject> AcquireTargetsRectWorld(Vector3 worldCenter, int halfSizeCells, TargetKind targetKind = TargetKind.Towers)
    {
        MapManager mapManager = MapManager.Instance;
        if (mapManager == null || !mapManager.IsReady)
        {
            List<GameObject> empty = new List<GameObject>();
            return empty;
        }
        Vector3 worldCenter2D = worldCenter;
        worldCenter2D.z = 0f;
        Vector3Int centerCell = mapManager.WorldToCell(worldCenter2D);
        return AcquireTargetsRectCells(centerCell, halfSizeCells, targetKind);
    }

    // 한 셀(센터 1칸만)
    private List<GameObject> AcquireTargetsAtCell(Vector3Int cell, TargetKind targetKind = TargetKind.Towers)
    {
        return AcquireTargetsRectCells(cell, 0, targetKind);
    }

    // ───────────────────────────────────────────────────────────────────────
    // 원형(월드 반경) 범위
    // 타워 셀 사각으로 1차 후보를 실제 거리로 필터
    // 몬스터 위치 거리로 바로 필터
    // ───────────────────────────────────────────────────────────────────────
    private List<GameObject> AcquireTargetsRadiusWorld(Vector3 worldCenter, float radiusWorld, TargetKind targetKind = TargetKind.Towers)
    {
        MapManager mapManager = MapManager.Instance;
        List<GameObject> resultObjects = new List<GameObject>(32);
        if (mapManager == null || !mapManager.IsReady) return resultObjects;

        Vector3 worldCenter2D = worldCenter;
        worldCenter2D.z = 0f;
        float radiusSquared = radiusWorld * radiusWorld;
        HashSet<GameObject> uniqueObjects = new HashSet<GameObject>();

        Vector3Int originCell;
        Vector3Int sizeCells;
        Vector3 cellSize;
        mapManager.GetNavFrame(out originCell, out sizeCells, out cellSize);

        float cellStep = Mathf.Max(Mathf.Abs(cellSize.x), Mathf.Abs(cellSize.y));
        int halfSizeCells = Mathf.Max(0, Mathf.CeilToInt(radiusWorld / Mathf.Max(cellStep, 0.0001f)));

        Vector3Int centerCell = mapManager.WorldToCell(worldCenter2D);

        if ((targetKind & TargetKind.Towers) != 0)
        {
            List<GameObject> towerCandidates = AcquireTargetsRectCells(centerCell, halfSizeCells, TargetKind.Towers);
            foreach (GameObject towerObject in towerCandidates)
            {
                if (towerObject == null || !towerObject.activeInHierarchy) continue;
                float distanceSquared = (towerObject.transform.position - worldCenter2D).sqrMagnitude;
                if (distanceSquared <= radiusSquared && uniqueObjects.Add(towerObject))
                    resultObjects.Add(towerObject);
            }
        }

        if ((targetKind & TargetKind.Monsters) != 0)
        {
            GameObject[] monsterObjects = GameObject.FindGameObjectsWithTag(monsterTag);
            foreach (GameObject monsterObject in monsterObjects)
            {
                if (monsterObject == null || !monsterObject.activeInHierarchy) continue;
                float distanceSquared = (monsterObject.transform.position - worldCenter2D).sqrMagnitude;
                if (distanceSquared <= radiusSquared && uniqueObjects.Add(monsterObject))
                    resultObjects.Add(monsterObject);
            }
        }

        return resultObjects;
    }

}


