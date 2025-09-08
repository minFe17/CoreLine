using UnityEngine;

public sealed class MonsterSlow : IMonsterSkillHandler, ISkillTargetingSpecProvider
{
    public string Id { get { return "MonsterSlow"; } }

    // === 효과(타워 힐) ===
    public void Apply(GameObject towerObject, in SkillManager.SelectedSkill skill)
    {
        Debug.Log("Use");
        int damageAmount = Mathf.RoundToInt(skill.Effect.Value);
        if (damageAmount <= 0) return;

        MonsterMover monster;
        //float duration =  나중에 시간 정해지면 추가
        if (towerObject.TryGetComponent<MonsterMover>(out monster))
        {
            monster.ApplySpeedModifier(MonsterMover.SpeedType.Skill, skill.Effect.Value, skill.Effect.Value);
            return;
        }
    }

    public SkillTargetingSpec GetSpec(in SkillManager.SelectedSkill skill)
    {
        SkillTargetingSpec spec = new SkillTargetingSpec();
        spec.Mode = TargetingMode.RadiusWorld;
        spec.HalfSizeCells = 0; // 3x3
        spec.RadiusWorld = 3f;
        spec.ValidTargets = SkillManager.TargetKind.Monsters;
        return spec;
    }
}
