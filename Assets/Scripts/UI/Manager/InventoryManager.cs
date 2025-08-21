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
    }
    private void CreateButtons()
    {
        List<InventoryData> data = DataManager.Instance.InventoryDatas;
        _unitButtons = new PoolingManager("UI/Prefabs/Button/Inventory/InventoryUnitButton", _content, data.Count);
        for(int i=0;i<data.Count;i++)
        {
            InventoryUnitButton btn = _unitButtons.Pop().GetComponent<InventoryUnitButton>();
            btn.Data = data[i];
        }
    }
}
