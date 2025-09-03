using UnityEngine;

public sealed class RangeHealSkill : ITowerSkillHandler, ISkillTargetingSpecProvider
{
    public string Id { get { return "RangeHeal"; } }

    // === 효과(타워 힐) ===
    public void Apply(GameObject towerObject, in SkillManager.SelectedSkill skill)
    {
        Debug.Log("Use");
        int healAmount = Mathf.RoundToInt(skill.Value);
        if (healAmount <= 0) return;

        Unit unit;
        if (towerObject.TryGetComponent<Unit>(out unit))
        {
            if (unit.IsDie) return;
            unit.Heal(healAmount);
            return;
        }
    }

    // === 타게팅 스펙(3x3, 타워만) ===
    public SkillTargetingSpec GetSpec(in SkillManager.SelectedSkill skill)
    {
        SkillTargetingSpec spec = new SkillTargetingSpec();
        spec.Mode = TargetingMode.RectCells;
        spec.HalfSizeCells = 1; // 3x3
        spec.RadiusWorld = 0f;
        spec.ValidTargets = SkillManager.TargetKind.Towers;
        return spec;
    }
}
