using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FirebaseGameData
{
    public List<FirebaseUnlockedUnit> UnlockedUnit;
    public List<string> UnlockedLaboratoryId;
    public int PlayerMoney;
    public int PlayerGem;
    public int PlayerInfinityKey;
    public List<ClearStage> ClearStage;
}
[System.Serializable]
public class FirebaseUnlockedUnit
{
    public string UnitType; // enum → string
    public int AttackDamageLevel = 1;
    public int HealthPointLevel = 1;
    public int AttackRangeLevel = 1;
    public int AttackSpeedLevel = 1;
}
[System.Serializable]
public class GameData
{
    public List<UnlockedUnit> UnlockedUnit;
    public List<string> UnlockedLaboratoryId;
    public int PlayerMoney;
    public int PlayerGem;
    public int PlayerInfinityKey;
    public List<ClearStage> ClearStage;
}
[System.Serializable]
public class UnlockedUnit
{
    public EUnitType UnitType; //이거 enum으로 매칭해줘야됨
    public int AttackDamageLevel = 1;
    public int HealthPointLevel = 1;
    public int AttackRangeLevel = 1;
    public int AttackSpeedLevel = 1;
}
[System.Serializable]
public class ClearStage
{
    public string StageId;
    public int MaxStarNum;
    public Star Star;
}

[System.Serializable]
public class Star
{
    public bool FirstStar;
    public bool SecondStar;
    public bool ThirdStar;
}