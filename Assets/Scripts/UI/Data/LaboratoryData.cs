using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using UnityEngine;
using JetBrains.Annotations;

[JsonConverter(typeof(StringEnumConverter))]
public enum LaboratoryType
{
    None, Attack, Defense, Utility
}
[JsonConverter(typeof(StringEnumConverter))]
public enum ValueType
{
    Add, Mul, Sub, Skill
}
[JsonConverter(typeof(StringEnumConverter))]
public enum TargetType 
{
    King, Unit, PlayTime, Money, TimeUnitCost, SpawnUnitCost, Monster
}
//TimeUnitCost, SpawnUnitCost는 인게임 재화임
[JsonConverter(typeof(StringEnumConverter))]
public enum TargetStatus
{
    HealthPoint, Shield, AttackDamage, AttackSpeed, Critical, CriticalProbability
}

[System.Serializable]
public struct LaboratoryData
{
    public string Id;
    public string Info;
    public LaboratoryType LaboratoryType; //enum으로 빼자
    public int Cost; //이거 게임 클리어하면 얻는 재화 = Gem
    public Effect Effect;
    public List<string> ParentsId;
}
[System.Serializable]
public struct Effect
{
    public float Value;
    public ValueType ValueType;
    public TargetType TargetType;
    public TargetStatus TargetStatus;
}