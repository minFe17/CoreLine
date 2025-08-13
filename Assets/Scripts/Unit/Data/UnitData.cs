using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitData
{
    [SerializeField] string _unitType;
    [SerializeField] List<UnitLevelData> _levelData;
}