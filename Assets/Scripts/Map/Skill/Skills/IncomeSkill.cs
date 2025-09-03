public sealed class IncomeMoneyHandler : IIncomeSkillHandler
{
    public TargetType TargetType { get { return TargetType.IncomeMoney; } }

    public void Apply(in SkillManager.SelectedSkill skill)
    {
        if (CostManager.Instance == null) return;
        CostManager.Instance.AddUnitFraction(skill.Value);   // À¯´Ö Áö°© +Value
    }
}

public sealed class IncomeSkillHandler : IIncomeSkillHandler
{
    public TargetType TargetType { get { return TargetType.IncomeSkill; } }

    public void Apply(in SkillManager.SelectedSkill skill)
    {
        if (CostManager.Instance == null) return;
        CostManager.Instance.AddSkillFraction(skill.Value);  // ½ºÅ³ Áö°© +Value
    }
}
