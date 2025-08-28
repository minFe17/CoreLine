using UnityEngine;
using System.Collections.Generic;

public class ShowUnitController : MonoBehaviour
{
    private Dictionary<EUnitType, GameObject> _units = new Dictionary<EUnitType, GameObject>();
    private EUnitType _turnOntheUnitType;

    private void Awake()
    {
        SettingUnits();
    }
    private void Start()
    {
        EventManager.Instance.Subscribe<EUnitType>("ChangeChoiceUnitData", TurnOnTheUnit);
        EventManager.Instance.Subscribe("Reset", ResetUnit);
    }
    private void SettingUnits()
    {
        UnitAnimationController[] units = GetComponentsInChildren<UnitAnimationController>();
        foreach (UnitAnimationController unit in units)
        {
            _units.Add(unit.UnitType, unit.gameObject);
            unit.gameObject.SetActive(false);
        }
    }
    private void TurnOnTheUnit(EUnitType type)
    {
        TurnOffTheUnit();
        _units[type].gameObject.SetActive(true);
        _turnOntheUnitType = type;
    }
    private void TurnOffTheUnit()
    {
        if (_turnOntheUnitType == EUnitType.King) return;
        _units[_turnOntheUnitType].gameObject.SetActive(false);
    }
    private void ResetUnit()
    {
        TurnOffTheUnit();
        _turnOntheUnitType = EUnitType.King;
    }
}
