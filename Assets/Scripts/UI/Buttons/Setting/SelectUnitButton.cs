using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using static UnityEngine.InputSystem.InputSettings;

public class SelectUnitButton : UnitButton
{
    bool _isSetting = false;

    protected override void OnClick()
    {
        base.OnClick();
        EventManager.Instance.Invoke<EUnitType>("ChangeChoiceUnitData", _data.UnitType);
        _isSetting = UnitManager.Instance.SettingUnits.ContainsKey(_data.UnitType);
        if (UnitManager.Instance.SettingUnits.Count == 8 && !_isSetting)
        {
            print("추가x");
            return;//패널띄우기
        }
        if (_isSetting)
        {
            EventManager.Instance.Invoke("DeleteSelectedUnit");
        }
        else
        {
            EventManager.Instance.Invoke("AddSelectedUnit");
        }
    }
}
