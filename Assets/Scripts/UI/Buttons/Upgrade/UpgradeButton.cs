using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TMPro;
using System;

public class UpgradeButton : BaseButton
{
    private UpgradeType _status;
    private UpgradeData _data;

    private Dictionary<string, TextMeshProUGUI> _texts = new Dictionary<string, TextMeshProUGUI>();


    public UpgradeType Status
    {
        get { return _status; }
        set 
        { 
            _status = value;
            _data = DataManager.Instance.GetUpgradeData(_status);
        }
    }
    protected override void OnClick()
    {
        base.OnClick();
        int money = 0;
        bool isMaxLevel = false;
        bool checkMoney = CheckMoney(out money,out isMaxLevel);
        if (isMaxLevel)
        {
            print("MAxLevel");
            UIManager.Instance.OpenPopUp(PopUpStatus.MaxLevelAlret);
            return;
           
        }
        else if (!checkMoney)
        {
            UIManager.Instance.OpenPopUp(PopUpStatus.NoMoneyAlret);
            return;
        }
        DataManager.Instance.GameData.PlayerMoney -= money;
        EventManager.Instance.Invoke("UpdateMoneyText");
        EventManager.Instance.Invoke<UpgradeType>("UpgradeUnit", _status);
        EventManager.Instance.Invoke<EUnitType>("ChangeChoiceUnitData", UnitManager.Instance.ChoiceUnit.UnitType);
    }
    protected void Start()
    {
        MatchText();
        ChangeText(UnitManager.Instance.ChoiceUnit.UnitType);
        ChangeColor();
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe<EUnitType>("ChangeChoiceUnitData", ChangeText);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("ChangeChoiceUnitData", (Action<EUnitType>)ChangeText);
    }
    private bool CheckMoney(out int money, out bool isMaxLevel)
    {
        UnlockedUnit unit = UnitManager.Instance.GetUnlockedUnit(UnitManager.Instance.ChoiceUnit.UnitType);
        money = 0;
        isMaxLevel = false;
        switch (_status)
        {
            case UpgradeType.HealthPoint:
                {
                    if (unit.HealthPointLevel >= _data.MaxLevel)
                    {
                        isMaxLevel = true;
                        return false;
                    }
                    money = _data.Cost[unit.HealthPointLevel-1];
                    if (DataManager.Instance.GameData.PlayerMoney < money)
                        return false;
                }
                break;
            case UpgradeType.AttackDamage:
                {
                    if (unit.AttackDamageLevel >= _data.MaxLevel)
                    {
                        isMaxLevel = true;
                        return false;
                    }
                    money = _data.Cost[unit.AttackDamageLevel - 1];
                    if (DataManager.Instance.GameData.PlayerMoney < money)
                        return false;
                }
                break;
            case UpgradeType.AttackSpeed:
                {
                    if (unit.AttackSpeedLevel >= _data.MaxLevel)
                    {
                        isMaxLevel = true;
                        return false;
                    }
                    money = _data.Cost[unit.AttackSpeedLevel - 1];
                    if (DataManager.Instance.GameData.PlayerMoney < money)
                        return false;
                }
                break;
            case UpgradeType.AttackRange:
                {
                    if (unit.AttackRangeLevel >= _data.MaxLevel)
                    {
                        isMaxLevel = true;
                        return false;
                    }
                    money = _data.Cost[unit.AttackRangeLevel - 1];
                    if (DataManager.Instance.GameData.PlayerMoney < money)
                        return false;
                }
                break;
        }
        return true;
    }
    private void ChangeColor()
    {
        UIColorApplier color = GetComponent<UIColorApplier>();
        
        switch((int)_status%2)
        {
            case 0:
                color.MyColorType = ColorType.Normal;
                break;
            case 1:
                color.MyColorType = ColorType.Light;
                break;
        }
    }
    private void MatchText()
    {
        TextMeshProUGUI[] text = GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var tmp in GetComponentsInChildren<TextMeshProUGUI>())
        {
            _texts[tmp.name] = tmp;
        }
    }
   
    private void ChangeText(EUnitType type)
    {
        UnlockedUnit unit = UnitManager.Instance.GetUnlockedUnit(type);
        print(_status);
        switch(_status)
        {
            case UpgradeType.HealthPoint:
                if (unit.HealthPointLevel < _data.MaxLevel)
                    _texts["PriceText"].text = _data.Cost[unit.HealthPointLevel - 1].ToString();
                else
                    _texts["PriceText"].text = "최대강화";
                _texts["LevelText"].text = "+" + unit.HealthPointLevel.ToString();
                _texts["InfoText"].text = _status.ToString();
                break;
            case UpgradeType.AttackDamage:
                if (unit.AttackDamageLevel < _data.MaxLevel)
                    _texts["PriceText"].text = _data.Cost[unit.AttackDamageLevel - 1].ToString();
                else
                    _texts["PriceText"].text = "최대강화";
                _texts["LevelText"].text = "+" + unit.AttackDamageLevel.ToString();
                _texts["InfoText"].text = _status.ToString();
                break;
            case UpgradeType.AttackSpeed:
                if (unit.AttackSpeedLevel < _data.MaxLevel)
                    _texts["PriceText"].text = _data.Cost[unit.AttackSpeedLevel - 1].ToString();
                else
                    _texts["PriceText"].text = "최대강화";
                _texts["LevelText"].text = "+" + unit.AttackSpeedLevel.ToString();
                _texts["InfoText"].text = _status.ToString();
                break;
            case UpgradeType.AttackRange:
                if (unit.AttackRangeLevel < _data.MaxLevel)
                    _texts["PriceText"].text = _data.Cost[unit.AttackDamageLevel - 1].ToString();
                else
                    _texts["PriceText"].text = "최대강화";
                _texts["LevelText"].text = "+"+unit.AttackRangeLevel.ToString();
                _texts["InfoText"].text = _status.ToString();
                break;
        }
    }

}
