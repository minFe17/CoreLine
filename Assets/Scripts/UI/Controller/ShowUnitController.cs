using TMPro;
using UnityEngine;
using System;

public class ShowUnitController: MonoBehaviour
{
    private bool _isBuyUnit = true;
    private TextMeshProUGUI _text;
    private Animator _animator;

    private void Start()
    {
        FindAndGetComponent();
    }
    private void OnEnable()
    {
        Debug.Log($"OnEnable 호출: {gameObject.name}");
        EventManager.Instance.Subscribe<EUnitType>("ChangeUnit", ChangeAnimation);
        EventManager.Instance.Subscribe<EUnitType>("ChangeUnit", ChangeText);
        EventManager.Instance.Subscribe<bool, EUnitType>("IsBuyUnit", IsBuyUnit);
    }
    private void OnDisable()
    {
        Debug.Log($"OnDisable 호출: {gameObject.name}");
        EventManager.Instance.UnSubscribe("ChangeUnit", (Action<EUnitType>)ChangeAnimation);
        EventManager.Instance.UnSubscribe("ChangeUnit", (Action<EUnitType>)ChangeText);
        EventManager.Instance.UnSubscribe("IsBuyUnit", (Action<bool, EUnitType>)IsBuyUnit);
    }
    private void IsBuyUnit(bool param,EUnitType type)
    {
        _isBuyUnit = param;
        ChangeText(type);
        ChangeAnimation(type);
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
        Transform trans = transform.Find("UnitAnimation");
        _animator = trans.GetComponent<Animator>();
        trans = transform.Find("InformationBox/InfoText");
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
    private void ChangeAnimation(EUnitType param)
    {
        if(!_isBuyUnit)
        {
            GameObject my = gameObject;
            _animator.SetInteger("Unit", MatchingUnit(0));
            return;
        }
        _animator.SetInteger("Unit", MatchingUnit(param));
    }
    private int MatchingUnit (EUnitType param)
    {
        switch (param)
        {
            case EUnitType.King:
                return 0;
            case EUnitType.Wizard:
                return 1;
            case EUnitType.Pirate:
                return 2;
            case EUnitType.Warrior:
                return 3;
            case EUnitType.Chef:
                return 4;
            case EUnitType.Archer:
                return 5;

        }
        return 0;
    }
}
