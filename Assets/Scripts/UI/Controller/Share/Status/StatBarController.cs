using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Utils;

public class StatBarController : MonoBehaviour
{
    private Dictionary<UpgradeType, Slider> _sliders = new Dictionary<UpgradeType, Slider>();

    private void Start()
    {
        SetSliders();
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe<EUnitType>("ChangeChoiceUnitData", ChangeStatus);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("ChangeChoiceUnitData", (Action<EUnitType>)ChangeStatus);
    }
    private void SetSliders()
    {
        Slider[] sliders = GetComponentsInChildren<Slider>();
        foreach (Slider sl in sliders)
        {
            string name = sl.gameObject.name;
            UpgradeType type = (UpgradeType)Enum.Parse(typeof(UpgradeType), name);
            _sliders.Add(type, sl);
            SetMaxValue(sl, type);
        }

    }
    private void ChangeStatus(EUnitType unit)
    {
        SetUnitValue(unit, UpgradeType.HealthPoint);
        SetUnitValue(unit, UpgradeType.AttackRange);
        SetUnitValue(unit, UpgradeType.AttackDamage);
        SetUnitValue(unit, UpgradeType.AttackSpeed);
    }
    private void SetUnitValue(EUnitType unitType, UpgradeType type)
    {
        //if (!_sliders.ContainsKey(type)) return;
        UpgradeData data = DataManager.Instance.GetUpgradeData(type);
        float multi = (data.MaxMultiplier- data.BaseMultiplier)/data.MaxLevel;
        switch (type)
        {
            case UpgradeType.HealthPoint:
                _sliders[type].value = SimpleSingleton<UnitDataList>.Instance.GetUnitData(unitType).LevelData[0].UnitState.HP
                    * ((multi * UnitManager.Instance.GetUnlockedUnit(unitType).HealthPointLevel)+data.BaseMultiplier);
                print("HP"+_sliders[type].value);
                break;
            case UpgradeType.AttackDamage:
                _sliders[type].value = SimpleSingleton<UnitDataList>.Instance.GetUnitData(unitType).LevelData[0].UnitState.AttackDamage
                    * ((multi * UnitManager.Instance.GetUnlockedUnit(unitType).AttackDamageLevel) + data.BaseMultiplier);
                print("AD" + _sliders[type].value);
                break;
            case UpgradeType.AttackSpeed:
                _sliders[type].value = SimpleSingleton<UnitDataList>.Instance.GetUnitData(unitType).LevelData[0].UnitState.AttackSpeed
                    * ((multi * UnitManager.Instance.GetUnlockedUnit(unitType).AttackSpeedLevel) + data.BaseMultiplier);
                print("AS" + _sliders[type].value);
                break;
            case UpgradeType.AttackRange:
                _sliders[type].value = SimpleSingleton<UnitDataList>.Instance.GetUnitData(unitType).LevelData[0].UnitState.AttackRange
                    * ((multi * UnitManager.Instance.GetUnlockedUnit(unitType).AttackRangeLevel) + data.BaseMultiplier);
                print("AR" + _sliders[type].value);
                break;
        }
    }
    private void SetMaxValue(Slider slider, UpgradeType type)
    {
        switch(type)
        {
            case UpgradeType.HealthPoint:
                slider.maxValue = 110 * DataManager.Instance.GetUpgradeData(type).MaxMultiplier;
                break;
            case UpgradeType.AttackDamage:
                slider.maxValue = SimpleSingleton<UnitDataList>.Instance.GetUnitData(EUnitType.King).LevelData[0].UnitState.AttackDamage 
                    * DataManager.Instance.GetUpgradeData(type).MaxMultiplier;
                break;
            case UpgradeType.AttackSpeed:
                slider.maxValue = SimpleSingleton<UnitDataList>.Instance.GetUnitData(EUnitType.King).LevelData[0].UnitState.AttackSpeed
                    * DataManager.Instance.GetUpgradeData(type).MaxMultiplier;
                break;
            case UpgradeType.AttackRange:
                slider.maxValue = SimpleSingleton<UnitDataList>.Instance.GetUnitData(EUnitType.King).LevelData[0].UnitState.AttackRange
                    * DataManager.Instance.GetUpgradeData(type).MaxMultiplier;
                break;
        }
    }
}
