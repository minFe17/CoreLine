using NUnit.Framework.Constraints;
using UnityEngine;

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
        _unitButtons = new PoolingManager("UI/Prefabs/Button/Inventory/InventoryUnitButton", _content, 20);
        for(int i=0;i<20;i++)
        {
            InventoryUnitButton btn = _unitButtons.Pop().GetComponent<InventoryUnitButton>();
        }
    }
}
