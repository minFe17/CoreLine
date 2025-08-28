using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class FusionDataList
{
    [SerializeField] List<FusionData> _fusionDataList;

    Dictionary<EFusionUnitType, FusionData> _fusionDataDict;

    public IReadOnlyList<FusionData> DataList => _fusionDataList;

    void Init()
    {
        _fusionDataDict = _fusionDataList.ToDictionary(unit => unit.UnitType);
    }

    public FusionData GetFusionData(EFusionUnitType unitType)
    {
        if (_fusionDataDict == null)
            Init();

        if (_fusionDataDict.TryGetValue(unitType, out FusionData unitData))
            return unitData;

        return null;
    }
}