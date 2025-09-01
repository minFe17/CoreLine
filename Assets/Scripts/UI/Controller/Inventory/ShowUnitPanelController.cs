using TMPro;
using UnityEngine;
using System;

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
        if (!_isBuyUnit)
        {
            InventoryData uData = UnitManager.Instance.GetInventoryData(param);
            _text.text = uData.UnlockPrice.ToString();
            return;
        }
        InventoryData data = UnitManager.Instance.GetInventoryData(param);
        _text.text = data.Information;
        _isBuyUnit = true;
    }
}
