public sealed class IncomeMoneyHandler : IIncomeSkillHandler
{
    public TargetType TargetType { get { return TargetType.TimeUnitCost; } }

    public void Apply(in SkillManager.SelectedSkill skill)
    {
        if (!SkillManager.Instance.TryGetSkillDef(skill.Id, out var def)) return;

        if (CostManager.Instance == null) return;

        CostManager.Instance.SetGainRate(CostManager.CostType.Unit,def.Value);   // À¯´Ö Áö°© +Value
        float timer = 0;
        
        CostManager.Instance.SetAutoGain(CostManager.CostType.Unit, false);
    }
}

public sealed class IncomeSkillHandler : IIncomeSkillHandler
{
    public TargetType TargetType { get { return TargetType.SpawnUnitCost; } }

    public void Apply(in SkillManager.SelectedSkill skill)
    {
        if (!SkillManager.Instance.TryGetSkillDef(skill.Id, out var def)) return;

        if (CostManager.Instance == null) return;
        CostManager.Instance.AddUnitFraction(def.Value);  // ½ºÅ³ Áö°© +Value
    }
}
