using TMPro;
using UnityEngine;
using System;

public class ShowUnitPanelController: MonoBehaviour
{
    private bool _isBuyUnit = true;
    private TextMeshProUGUI _text;

    private void Start()
    {
        FindAndGetComponent();
    }
    private void OnEnable()
    {
        Debug.Log($"OnEnable 호출: {gameObject.name}");
        //EventManager.Instance.Subscribe<EUnitType>("ChangeUnit", ChangeText);
        EventManager.Instance.Subscribe<bool, EUnitType>("IsBuyUnit", IsBuyUnit);
    }
    private void OnDisable()
    {
        Debug.Log($"OnDisable 호출: {gameObject.name}");
        //EventManager.Instance.UnSubscribe("ChangeUnit", (Action<EUnitType>)ChangeText);
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
