using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitDataList
{
    [SerializeField] List<UnitData> _unitList;

    public List<UnitData> UnitList { get => _unitList; }
}