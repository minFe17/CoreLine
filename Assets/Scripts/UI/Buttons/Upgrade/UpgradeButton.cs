using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class UpgradeButton : BaseButton
{
    [SerializeField]
    private UpgradeType _status;

    private List<UnlockedUnit> _units;

    protected override void OnClick()
    {
        UpgradeUnit();
    }
    protected void Start()
    {
        _units = UnitManager.Instance.UnlockedUnits;
    }
    private void UpgradeUnit()
    {
        //돈먼저 확인 (안되면 패널)할 것
        EventManager.Instance.Invoke<UpgradeType>("UpgradeUnit", _status);
    }

}
