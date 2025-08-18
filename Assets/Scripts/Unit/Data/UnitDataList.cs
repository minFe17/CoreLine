using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class UnitDataList
{
    // ╫л╠шео
    [SerializeField] List<UnitData> _unitList;

    Dictionary<EUnitType, UnitData> _unitDict;

    void Init()
    {
        _unitDict = _unitList.ToDictionary(unit => unit.UnitType);
    }

    public UnitData GetUnitData(EUnitType unitType)
    {
        if (_unitDict == null)
            Init();

        if (_unitDict.TryGetValue(unitType, out UnitData unitData))
            return unitData;

        return null;
    }
}