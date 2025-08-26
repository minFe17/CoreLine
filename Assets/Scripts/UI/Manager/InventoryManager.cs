using NUnit.Framework;
using NUnit.Framework.Constraints;
using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    private PoolingManager _unitButtons;
    private GameObject _content;
    private void Start()
    {
        _content = GameObject.Find("Content");
        CreateButtons();
        EventManager.Instance.Subscribe("SettingBuyUnit", UpdateUnitButton);
    }
    private void CreateButtons()
    {
        List<InventoryData> data = DataManager.Instance.InventoryDatas;

        HashSet<EUnitType> unlockedTypes = new HashSet<EUnitType>();
        foreach (UnlockedUnit unlocked in UnitManager.Instance.UnlockedUnits)
        {
            unlockedTypes.Add(unlocked.UnitType);
        }

        List<InventoryData> sortedData = new List<InventoryData>(data);
        sortedData.Sort((a, b) =>
        {
            bool aUnlocked = unlockedTypes.Contains(a.UnitType);
            bool bUnlocked = unlockedTypes.Contains(b.UnitType);

            if (aUnlocked == bUnlocked)
                return 0;
            else if (aUnlocked)
                return -1;
            else
                return 1;
        });

        _unitButtons = new PoolingManager("UI/Prefabs/Button/Inventory/InventoryUnitButton", _content, sortedData.Count);
        for (int i = 0; i < sortedData.Count; i++)
        {
            InventoryUnitButton btn = _unitButtons.Pop().GetComponent<InventoryUnitButton>();
            btn.Data = sortedData[i];
        }
    }
    private void UpdateUnitButton()
    {
        foreach(GameObject obj in _unitButtons.GetAllToActiveTrue())
        {
            obj.SetActive(false);
        }
        CreateButtons();
    }
}
