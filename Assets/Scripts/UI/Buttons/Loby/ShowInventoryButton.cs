using UnityEngine;

public class ShowInventoryButton : BaseButton
{
    protected override void OnClick()
    {
        UIManager.Instance.AddPanelStack(PanelStatus.InventoryPanel);
        //UIManager.Instance.AddPanelStack(PanelStatus.SettingPanel);
    }

}
