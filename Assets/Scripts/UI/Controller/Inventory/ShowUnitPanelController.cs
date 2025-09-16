using System;
using TMPro;
using UnityEngine;
using Utils;
using static UnityEngine.Analytics.IAnalytic;

public class ShowUnitPanelController: MonoBehaviour
{
    private bool _isBuyUnit = true;
    private bool _isStart = false;
    private TextMeshProUGUI _text;


    private void Start()
    {
        FindAndGetComponent();
        _isStart = true;
        EventManager.Instance.Invoke<bool, EUnitType>("IsBuyUnit", UnitManager.Instance.IsGetUnit(UnitManager.Instance.ChoiceUnit.UnitType), UnitManager.Instance.ChoiceUnit.UnitType);
        EventManager.Instance.Invoke<EUnitType>("ChangeChoiceUnitData", UnitManager.Instance.ChoiceUnit.UnitType);
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe<bool, EUnitType>("IsBuyUnit", IsBuyUnit);
        if(_isStart)
        {
            EventManager.Instance.Invoke<bool, EUnitType>("IsBuyUnit", UnitManager.Instance.IsGetUnit(UnitManager.Instance.ChoiceUnit.UnitType), UnitManager.Instance.ChoiceUnit.UnitType);
            EventManager.Instance.Invoke<EUnitType>("ChangeChoiceUnitData", UnitManager.Instance.ChoiceUnit.UnitType);
        }  
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("IsBuyUnit", (Action<bool, EUnitType>)IsBuyUnit);
    }
    private void IsBuyUnit(bool param,EUnitType type)
    {
        _isBuyUnit = param;
        ChangeText(type);
    }
    //private void FindAndGetComponent()
    //{
    //    _animator = GetComponentInChildren<Animator>();
    //    Transform trans = transform.Find("InformationBox/InfoText");
    //    if (trans != null)
    //    {
    //        _text = trans.GetComponent<TextMeshProUGUI>();
    //    }
    //}
    private void FindAndGetComponent()
    {
        Transform trans = transform.Find("InformationBox/InfoText");
        if (trans != null)
        {
            _text = trans.GetComponent<TextMeshProUGUI>();
        }
    }
    private void ChangeText(EUnitType param)
    {
        if (_text == null) return;

        InventoryData data = UnitManager.Instance.GetInventoryData(param);
        string text = data.UnlockPrice + " 코인\nHealthPoint : " + SimpleSingleton<UnitDataList>.Instance.GetUnitData(param).LevelData[0].UnitState.HP
    + "\nAttackDamage : " + SimpleSingleton<UnitDataList>.Instance.GetUnitData(param).LevelData[0].UnitState.AttackDamage
    + "\nAttackRange : " + SimpleSingleton<UnitDataList>.Instance.GetUnitData(param).LevelData[0].UnitState.AttackRange
    + "\nAttackSpeed : " + SimpleSingleton<UnitDataList>.Instance.GetUnitData(param).LevelData[0].UnitState.AttackSpeed;
        //여기 해야됨(해제조건있는지없는지)
        if (!_isBuyUnit)
        {
            _text.text = text;
            return;
        }
        GameData udata = DataManager.Instance.GameData;
        foreach (var unit in udata.UnlockedUnit)
        {
            if (unit.UnitType == param)
            {
                text = data.Information+"\nHP Level. " + unit.HealthPointLevel + "\nAttackDamageLevel. " + unit.AttackDamageLevel
                    + "\nAttackRangeLevel. " + unit.AttackRangeLevel + "\nAttackSpeedLevel. " + unit.AttackSpeedLevel;
            }

        }
        _text.text = text;
        _isBuyUnit = true;
    }
}
