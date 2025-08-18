using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradeData
{
    [SerializeField] string _upgradeType;
    [SerializeField] float _baseMultiplier;
    [SerializeField] float _maxMultiplier;
    [SerializeField] int _baseLevel;
    [SerializeField] int _maxLevel;
    [SerializeField] List<int> _cost;

}