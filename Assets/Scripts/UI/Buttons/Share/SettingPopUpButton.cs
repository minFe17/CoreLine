using UnityEngine;

public class SettingPopUpButton : BaseButton
{
    protected override void OnClick()
    {
        UIManager.Instance.OpenPopUp(PopUpStatus.SettingPopUp);
    }

}
