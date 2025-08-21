using System.Collections.Generic;
using UnityEngine;

public class SettingPanelManager : MonoBehaviour
{
    private PoolingManager _unitButtons;
    private GameObject _content;
    private void Start()
    {
        _content = GameObject.Find("Content");
        CreateButtons();
    }
    private void CreateButtons()
    {
        List<UnlockedUnit> data = UnitManager.Instance.UnlockedUnits;
        _unitButtons = new PoolingManager("UI/Prefabs/Button/Setting/SettingUnitButton", _content, UnitManager.Instance.AllUnitCount());
        for (int i = 0; i < data.Count; i++)
        {
            if (data[i].Type == "King") continue;
            SettingUnitButton btn = _unitButtons.Pop().GetComponent<SettingUnitButton>();
            btn.Data = UnitManager.Instance.GetInventoryData(data[i].Type);
            print(btn.Data.UnitType);
        }
    }
}
