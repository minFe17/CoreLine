using UnityEngine;

public enum TargetingMode { Point, RectCells, RadiusWorld }

public struct SkillTargetingSpec
{
    public TargetingMode Mode;                  // 포인트/사각(셀)/원형(월드)
    public int HalfSizeCells;                   // RectCells 전용 (1 => 3x3)
    public float RadiusWorld;                   // RadiusWorld 전용
    public SkillManager.TargetKind ValidTargets; // Towers / Monsters / Both (프리뷰 색 결정 등에 활용)
}

// 타워(아군 유닛/타워) 대상 스킬
public interface ITowerSkillHandler
{
    string Id { get; } // 스킬 ID (예: "RangeHeal")
    void Apply(GameObject towerObject, in SkillManager.SelectedSkill skill);
}

// 몬스터 대상 스킬
public interface IMonsterSkillHandler
{
    string Id { get; } // 스킬 ID (예: "RangeNuke")
    void Apply(GameObject monsterObject, in SkillManager.SelectedSkill skill);
}

// 인컴(보상) 스킬: TargetType에 따라 동작 (IncomeMoney/IncomeSkill)
public interface IIncomeSkillHandler
{
    TargetType TargetType { get; } // IncomeMoney / IncomeSkill
    void Apply(in SkillManager.SelectedSkill skill);
}

// UI가 “이 스킬은 어떻게 겨냥할까?”를 알기 위한 제공자
public interface ISkillTargetingSpecProvider
{
    string Id { get; }
    SkillTargetingSpec GetSpec(in SkillManager.SelectedSkill skill);
}
