using UnityEngine;

public class ShowSettingButton : BaseButton
{
    protected override void OnClick()
    {
        UIManager.Instance.AddPanelStack(PanelStatus.SettingPanel);
        //UIManager.Instance.AddPanelStack(PanelStatus.SettingPanel);
    }

}
