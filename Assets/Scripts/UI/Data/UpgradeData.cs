using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using UnityEngine;

[JsonConverter(typeof(StringEnumConverter))]
public enum UpgradeType
{
    HealthPoint, AttackDamage, AttackRange, AttackSpeed
}

[System.Serializable]
public struct UpgradeData
{
    public UpgradeType UpgradeType;
    public float BaseMultiplier;
    public float MaxMultiplier;
    public int BaseLevel;
    public int MaxLevel;
    public List<int> Cost;
}