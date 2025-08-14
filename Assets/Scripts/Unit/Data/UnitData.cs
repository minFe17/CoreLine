using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitData
{
    [SerializeField] string _unitType;
    [SerializeField] List<UnitLevelData> _levelData;

    public EUnitType UnitType { get => (EUnitType)Enum.Parse(typeof(EUnitType), _unitType); }
    public List<UnitLevelData> LevelData { get => _levelData; }
}