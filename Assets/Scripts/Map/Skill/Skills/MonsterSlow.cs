using UnityEngine;

public sealed class MonsterSlow : IMonsterSkillHandler, ISkillTargetingSpecProvider
{
    public string Id { get { return "MonsterSlow"; } }

    // === 효과(타워 힐) ===
    public void Apply(GameObject towerObject, in SkillManager.SelectedSkill skill)
    {
        if (!SkillManager.Instance.TryGetSkillDef(skill.Id, out var def)) return;

        int damageAmount = Mathf.RoundToInt(def.Value);

        MonsterMover monster;
        //float duration =  나중에 시간 정해지면 추가
        if (towerObject.TryGetComponent<MonsterMover>(out monster))
        {
            monster.ApplySpeedModifier(MonsterMover.SpeedType.Skill, def.Value, def.Duration);
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
