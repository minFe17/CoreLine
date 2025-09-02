using System;
using UnityEngine;
using Utils;

public class CostManager : MonoSingleton<CostManager>
{
    public enum CostType
    {
        Unit,
        Skill
    }

    [Header("Unit Placement Budget")]
    [SerializeField] private bool unitAutoGain = true;
    [SerializeField] private float unitGainPerSecond = 2f;
    [SerializeField] private bool unitUseUnscaledTime = false;
    [SerializeField] private int unitStartValue = 0;

    [Header("Skill Budget")]
    [SerializeField] private bool skillAutoGain = true;
    [SerializeField] private float skillGainPerSecond = 2f;
    [SerializeField] private bool skillUseUnscaledTime = false;
    [SerializeField] private int skillStartValue = 0;

    public int CurrentUnit { get; private set; }
    public int CurrentSkill { get; private set; }

    public event Action<int> OnUnitChanged;
    public event Action<int> OnSkillChanged;

    private float unitCarry;   // 유닛 지갑 소수 누적
    private float skillCarry;  // 스킬 지갑 소수 누적

    private void OnEnable()
    {
        CurrentUnit = Mathf.Max(0, unitStartValue);
        CurrentSkill = Mathf.Max(0, skillStartValue);
        Action<int> unitChanged = OnUnitChanged;
        if (unitChanged != null) unitChanged.Invoke(CurrentUnit);
        Action<int> skillChanged = OnSkillChanged;
        if (skillChanged != null) skillChanged.Invoke(CurrentSkill);
    }

    private void Update()
    {
        float deltaTimeUnit = unitUseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        float deltaTimeSkill = skillUseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (unitAutoGain && unitGainPerSecond > 0f)
        {
            unitCarry += unitGainPerSecond * deltaTimeUnit;
            if (unitCarry >= 1f)
            {
                int increase = Mathf.FloorToInt(unitCarry);
                unitCarry -= increase;
                AddUnit(increase);
            }
        }

        if (skillAutoGain && skillGainPerSecond > 0f)
        {
            skillCarry += skillGainPerSecond * deltaTimeSkill;
            if (skillCarry >= 1f)
            {
                int increase = Mathf.FloorToInt(skillCarry);
                skillCarry -= increase;
                AddSkill(increase);
            }
        }
    }

    // ─────────────────────────────
    // 유닛 지갑 API
    // ─────────────────────────────
    public void AddUnit(int amount)
    {
        if (amount == 0) return;
        CurrentUnit = Mathf.Max(0, CurrentUnit + amount);
        Action<int> cb = OnUnitChanged;
        if (cb != null) cb.Invoke(CurrentUnit);
    }

    // 소수 누적 버전(새 이름)
    public void AddUnitFraction(float amount)
    {
        unitCarry += amount;
        if (unitCarry >= 1f)
        {
            int increase = Mathf.FloorToInt(unitCarry);
            unitCarry -= increase;
            AddUnit(increase);
        }
    }

    public void SetUnitValue(int value)
    {
        CurrentUnit = Mathf.Max(0, value);
        Action<int> cb = OnUnitChanged;
        if (cb != null) cb.Invoke(CurrentUnit);
    }

    public bool TrySpendUnit(int amount)
    {
        if (amount <= 0) return true;
        if (CurrentUnit < amount) return false;
        CurrentUnit -= amount;
        Action<int> cb = OnUnitChanged;
        if (cb != null) cb.Invoke(CurrentUnit);
        return true;
    }

    public void SpendUnitForce(int amount)
    {
        if (amount <= 0) return;
        CurrentUnit = Mathf.Max(0, CurrentUnit - amount);
        Action<int> cb = OnUnitChanged;
        if (cb != null) cb.Invoke(CurrentUnit);
    }

    public void SetUnitAutoGain(bool enabled) { unitAutoGain = enabled; }
    public void SetUnitGainRate(float perSecond) { unitGainPerSecond = Mathf.Max(0f, perSecond); }

    // ─────────────────────────────
    // 스킬 지갑 API
    // ─────────────────────────────
    public void AddSkill(int amount)
    {
        if (amount == 0) return;
        CurrentSkill = Mathf.Max(0, CurrentSkill + amount);
        Action<int> cb = OnSkillChanged;
        if (cb != null) cb.Invoke(CurrentSkill);
    }

    // 소수 누적 버전(새 이름)
    public void AddSkillFraction(float amount)
    {
        skillCarry += amount;
        if (skillCarry >= 1f)
        {
            int increase = Mathf.FloorToInt(skillCarry);
            skillCarry -= increase;
            AddSkill(increase);
        }
    }

    public void SetSkillValue(int value)
    {
        CurrentSkill = Mathf.Max(0, value);
        Action<int> cb = OnSkillChanged;
        if (cb != null) cb.Invoke(CurrentSkill);
    }

    public bool TrySpendSkill(int amount)
    {
        if (amount <= 0) return true;
        if (CurrentSkill < amount) return false;
        CurrentSkill -= amount;
        Action<int> cb = OnSkillChanged;
        if (cb != null) cb.Invoke(CurrentSkill);
        return true;
    }

    public void SpendSkillForce(int amount)
    {
        if (amount <= 0) return;
        CurrentSkill = Mathf.Max(0, CurrentSkill - amount);
        Action<int> cb = OnSkillChanged;
        if (cb != null) cb.Invoke(CurrentSkill);
    }

    public void SetSkillAutoGain(bool enabled) { skillAutoGain = enabled; }
    public void SetSkillGainRate(float perSecond) { skillGainPerSecond = Mathf.Max(0f, perSecond); }

    // ─────────────────────────────
    // 공통(타입 기반) API
    // ─────────────────────────────
    public int GetCurrent(CostType type)
    {
        return type == CostType.Unit ? CurrentUnit : CurrentSkill;
    }

    public void Add(CostType type, int amount)
    {
        if (type == CostType.Unit) AddUnit(amount);
        else AddSkill(amount);
    }

    public void Add(CostType type, float amount)
    {
        if (type == CostType.Unit) AddUnitFraction(amount);
        else AddSkillFraction(amount);
    }

    public void SetValue(CostType type, int value)
    {
        if (type == CostType.Unit) SetUnitValue(value);
        else SetSkillValue(value);
    }

    public bool TrySpend(CostType type, int amount)
    {
        return type == CostType.Unit ? TrySpendUnit(amount) : TrySpendSkill(amount);
    }

    public void SpendForce(CostType type, int amount)
    {
        if (type == CostType.Unit) SpendUnitForce(amount);
        else SpendSkillForce(amount);
    }

    public void SetAutoGain(CostType type, bool enabled)
    {
        if (type == CostType.Unit) SetUnitAutoGain(enabled);
        else SetSkillAutoGain(enabled);
    }

    public void SetGainRate(CostType type, float perSecond)
    {
        if (type == CostType.Unit) SetUnitGainRate(perSecond);
        else SetSkillGainRate(perSecond);
    }
}
