using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public List<UnlockedUnit> UnlockedUnit;
    public List<string> UnlockedLaboratoryId;
    public int PlayerMoney;
}
[System.Serializable]
public class UnlockedUnit
{
    public EUnitType UnitType; //이거 enum으로 매칭해줘야됨
    public int AttackDatamageLevel;
    public int HealthPointLevel;
    public int AttackRangeLevel;
}