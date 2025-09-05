using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TMPro;

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
        UpgradeUnit();
        ChangeText();
    }
    protected void Start()
    {
        MatchText();
        ChangeText();
        ChangeColor();
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
   
    private void ChangeText()
    {
        UnlockedUnit unit = UnitManager.Instance.GetUnlockedUnit(UnitManager.Instance.ChoiceUnit.UnitType);
        print(_status);
        switch(_status)
        {
            case UpgradeType.HealthPoint:
                _texts["PriceText"].text = _data.Cost[unit.HealthPointLevel].ToString();
                _texts["LevelText"].text = "+" + unit.HealthPointLevel.ToString();
                _texts["InfoText"].text = _status.ToString();
                break;
            case UpgradeType.AttackDamage:
                _texts["PriceText"].text = _data.Cost[unit.AttackDamageLevel].ToString();
                _texts["LevelText"].text = "+" + unit.AttackDamageLevel.ToString();
                _texts["InfoText"].text = _status.ToString();
                break;
            case UpgradeType.AttackSpeed:
                _texts["PriceText"].text = _data.Cost[unit.AttackSpeedLevel].ToString();
                _texts["LevelText"].text = "+" + unit.AttackSpeedLevel.ToString();
                _texts["InfoText"].text = _status.ToString();
                break;
            case UpgradeType.AttackRange:
                _texts["PriceText"].text = _data.Cost[unit.AttackRangeLevel].ToString();
                _texts["LevelText"].text = "+"+unit.AttackRangeLevel.ToString();
                _texts["InfoText"].text = _status.ToString();
                break;
        }
    }

    private void UpgradeUnit()
    {
        //돈먼저 확인 (안되면 패널)할 것
        EventManager.Instance.Invoke<UpgradeType>("UpgradeUnit", _status);
        EventManager.Instance.Invoke<EUnitType>("ChangeChoiceUnitData", UnitManager.Instance.ChoiceUnit.UnitType);
        //이거 스탯 창 바로바로업데이트해주나??
    }

}
