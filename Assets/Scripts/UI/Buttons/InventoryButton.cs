using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryButton : BaseButton
{
    protected override void OnClick()
    {
        UIManager.Instance.AddPanelStack(PanelStatus.InventoryPanel);
    }
}