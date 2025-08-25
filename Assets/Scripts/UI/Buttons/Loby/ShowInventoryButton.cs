using UnityEngine;

public class ShowInventoryButton : BaseButton
{
    public void OnClickInventory()
    {
        UIManager.Instance.AddPanelStack(PanelStatus.InventoryPanel);
    }
    public void OnClickSetting()
    {
        UIManager.Instance.AddPanelStack(PanelStatus.SettingPanel);
    }    
    protected override void OnClick()
    {
        UIManager.Instance.AddPanelStack(PanelStatus.InventoryPanel);
        //UIManager.Instance.AddPanelStack(PanelStatus.SettingPanel);
    }

}
