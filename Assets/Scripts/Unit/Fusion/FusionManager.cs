using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public class FusionManager : MonoBehaviour
{
    // ╫л╠шео
    Dictionary<EUnitType, List<TowerUnit>> _fusionableUnit = new Dictionary<EUnitType, List<TowerUnit>>();
    List<EUnitType> _targetTypeList = new List<EUnitType>();
    TowerUnit _baseUnit;

    bool TryGetFusionableTargetTypes(EUnitType selectedType, out List<EUnitType> targetTypes)
    {
        FusionDataList data = SimpleSingleton<FusionDataList>.Instance;

        targetTypes = data.DataList
            .Where(fusion => fusion.MaterialUnit.Contains(selectedType))
            .Select(fusion => fusion.MaterialUnit.First(type => type != selectedType))
            .ToList();

        return targetTypes.Count > 0;
    }

    bool HasFusionableUnits()
    {
        foreach (EUnitType targetType in _targetTypeList)
        {
            if (_fusionableUnit.TryGetValue(targetType, out List<TowerUnit> units) && units.Count > 0)
                return true;
        }
        return false;
    }

    void ApplyFusionLayer(string layerName)
    {
        foreach (EUnitType targetType in _targetTypeList)
        {
            if (_fusionableUnit.TryGetValue(targetType, out List<TowerUnit> units) == false)
                continue;

            foreach (TowerUnit unit in units)
                unit.SetFusionLayer("FusionUnit");
        }
        _baseUnit.SetFusionLayer("FusionUnit");
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
        _targetTypeList.Clear();
        if (!TryGetFusionableTargetTypes(unit.UnitType, out _targetTypeList))
            return;

        bool hasFusionableUnit = HasFusionableUnits();

        if (!hasFusionableUnit)
            return;

        _baseUnit = unit;
        ApplyFusionLayer("FusionUnit");
        SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorType.Fusion, true);
    }

    public void Fusion(TowerUnit unit)
    {
        if (_baseUnit == null)
        {
            FindFusionUnits(unit);
            return;
        }
        if (_baseUnit == unit)
            return;

        FusionDataList data = SimpleSingleton<FusionDataList>.Instance;

        FusionData fusionData = data.DataList.FirstOrDefault(fusion => fusion.MaterialUnit.Contains(unit.UnitType) && fusion.MaterialUnit.Contains(_baseUnit.UnitType) 
                                              && unit.UnitType != _baseUnit.UnitType);


        if (fusionData == null)
        {
            ApplyFusionLayer("Default");
            FindFusionUnits(unit);
            return;
        }

        GameObject temp = MonoSingleton<ObjectPoolManager>.Instance.Pull(fusionData.UnitType);
        temp.transform.position = _baseUnit.transform.position;

        Debug.Log(1);
        MonoSingleton<ObjectPoolManager>.Instance.Push(_baseUnit.UnitType, _baseUnit.gameObject);
        MonoSingleton<ObjectPoolManager>.Instance.Push(unit.UnitType, unit.gameObject);
        _baseUnit = null;

        SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorType.Fusion, false);
    }
}