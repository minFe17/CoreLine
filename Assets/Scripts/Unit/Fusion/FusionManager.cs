using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public class FusionManager : MonoBehaviour
{
    // ╫л╠шео
    Dictionary<EUnitType, List<TowerUnit>> _fusionableUnit = new Dictionary<EUnitType, List<TowerUnit>>();

    bool TryGetFusionableTargetTypes(EUnitType selectedType, out List<EUnitType> targetTypes)
    {
        FusionDataList data = SimpleSingleton<FusionDataList>.Instance;

        targetTypes = data.DataList
            .Where(fusion => fusion.MaterialUnit.Contains(selectedType))
            .Select(fusion => fusion.MaterialUnit.First(type => type != selectedType))
            .ToList();

        return targetTypes.Count > 0;
    }

    bool ApplyFusionLayerToUnits(List<EUnitType> targetTypes)
    {
        bool hasFusionableUnit = false;

        foreach (EUnitType targetType in targetTypes)
        {
            if (!_fusionableUnit.TryGetValue(targetType, out List<TowerUnit> units))
                continue;

            if (units.Count == 0)
                continue;

            foreach (TowerUnit unit in units)
                unit.SetFusionLayer();

            hasFusionableUnit = true;
        }
        return hasFusionableUnit;
    }

    public void AddFusionableUnit(EUnitType key, TowerUnit value)
    {
        if (!_fusionableUnit.TryGetValue(key, out List<TowerUnit> list))
        {
            list = new List<TowerUnit>();
            _fusionableUnit[key] = list;
        }
        list.Add(value);
    }

    public void FindFusionUnits(TowerUnit unit)
    {
        if (!TryGetFusionableTargetTypes(unit.UnitType, out List<EUnitType> targetTypes))
            return;

        bool hasFusionableUnit = ApplyFusionLayerToUnits(targetTypes);

        if (!hasFusionableUnit)
            return;

        unit.SetFusionLayer();
        SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorType.Fusion, true);
    }
}