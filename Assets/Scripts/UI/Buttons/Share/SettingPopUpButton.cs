using UnityEngine;

public class SettingPopUpButton : BaseButton
{
    protected override void OnClick()
    {
        base.OnClick();
        UIManager.Instance.OpenPopUp(PopUpStatus.SettingPopUp);
    }

}
