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
    bool _isFusionMode;

    public TowerUnit BaseUnit { get => _baseUnit; }
    public bool IsFusionMode { get => _isFusionMode; }

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
                unit.SetFusionLayer(layerName);
        }
        _baseUnit.SetFusionLayer(layerName);
    }

    FusionData GetFusionData(TowerUnit unitA, TowerUnit unitB)
    {
        IReadOnlyList<FusionData> dataList = SimpleSingleton<FusionDataList>.Instance.DataList;

        return dataList.FirstOrDefault(fusion =>
            fusion.MaterialUnit.Contains(unitA.UnitType) &&
            fusion.MaterialUnit.Contains(unitB.UnitType) &&
            unitA.UnitType != unitB.UnitType);
    }

    void HandleFailedFusion(TowerUnit unit)
    {
        ApplyFusionLayer("Default");
        FindFusionUnits(unit);
    }

    FusionUnit CreateFusedUnit(FusionData fusionData, Vector3 position)
    {
        SimpleSingleton<AttackRangeManager>.Instance.HideAttackRange();
        FusionUnit fusionUnit = MonoSingleton<ObjectPoolManager>.Instance.Pull(fusionData.UnitType).GetComponent<FusionUnit>();
        HpBar hpBar = MonoSingleton<ObjectPoolManager>.Instance.Pull(EUIPrefabType.UnitHpBar).GetComponent<HpBar>();
        SimpleSingleton<MapUnitManager>.Instance.AddUnit(_baseUnit.Cell, fusionUnit.GetComponent<Unit>());

        fusionUnit.transform.position = position;
        hpBar.SetPosition(position);

        fusionUnit.HpBar = hpBar;

        fusionUnit.Cell = _baseUnit.Cell;
        return fusionUnit;
    }

    void ReturnUnitsToPool(TowerUnit unitA, TowerUnit unitB)
    {
        unitA.UnregisterCell();
        unitB.UnregisterCell();

        _fusionableUnit[unitA.UnitType].Remove(unitA);
        _fusionableUnit[unitB.UnitType].Remove(unitB);

        MonoSingleton<ObjectPoolManager>.Instance.Push(unitA.UnitType, unitA.gameObject);
        MonoSingleton<ObjectPoolManager>.Instance.Push(unitB.UnitType, unitB.gameObject);
        MonoSingleton<ObjectPoolManager>.Instance.Push(EUIPrefabType.UnitHpBar, unitA.HpBar.gameObject);
        MonoSingleton<ObjectPoolManager>.Instance.Push(EUIPrefabType.UnitHpBar, unitB.HpBar.gameObject);
    }

    void NotifyFusion(bool value)
    {
        SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorType.Fusion, value);
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
        NotifyFusion(true);
        _isFusionMode = true;
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

        FusionData fusionData = GetFusionData(_baseUnit, unit);

        if (fusionData == null)
        {
            HandleFailedFusion(unit);
            return;
        }

        FusionUnit fusionUnit = CreateFusedUnit(fusionData, _baseUnit.transform.position);
        ApplyFusionLayer("Default");
        ReturnUnitsToPool(_baseUnit, unit);

        _baseUnit = null;
        MapManager.Instance.RegisterTower(fusionUnit.Cell, fusionUnit.gameObject);
        SimpleSingleton<MapUnitManager>.Instance.AddUnit(fusionUnit.Cell, fusionUnit.GetComponent<Unit>());

        NotifyFusion(false);
        _isFusionMode = false;
    }
}