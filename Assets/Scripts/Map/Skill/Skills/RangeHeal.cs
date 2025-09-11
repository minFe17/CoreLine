using UnityEngine;

public sealed class RangeHeal : ITowerSkillHandler, ISkillTargetingSpecProvider
{
    public string Id { get { return "RangeHeal"; } }

    // === 효과(타워 힐) ===
    public void Apply(GameObject towerObject, in SkillManager.SelectedSkill skill)
    {
        if (!SkillManager.Instance.TryGetSkillDef(skill.Id, out var def)) return;

        int healAmount = Mathf.RoundToInt(def.Value);
        if (healAmount <= 0) return;

        if (towerObject.TryGetComponent<Unit>(out var unit))
        {
            unit.Heal(healAmount);
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
