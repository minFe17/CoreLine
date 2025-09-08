using UnityEngine;

public sealed class ArrowRain : IMonsterSkillHandler, ISkillTargetingSpecProvider
{
    public string Id { get { return "ArrowRain"; } }

    // === 효과(타워 힐) ===
    public void Apply(GameObject towerObject, in SkillManager.SelectedSkill skill)
    {
        Debug.Log("Use");
        int damageAmount = Mathf.RoundToInt(skill.Effect.Value);
        if (damageAmount <= 0) return;

        Monster monster;
        if (towerObject.TryGetComponent<Monster>(out monster))
        {
            monster.TakeDamage(damageAmount);
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
        spec.ValidTargets = SkillManager.TargetKind.Monsters;
        return spec;
    }
}
