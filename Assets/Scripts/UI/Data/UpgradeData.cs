using System.Collections.Generic;
using UnityEngine;

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