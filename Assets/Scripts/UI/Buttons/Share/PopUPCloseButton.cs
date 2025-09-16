using UnityEngine;

public class PopUPCloseButton : BaseButton
{
    protected override void OnClick()
    {
        base.OnClick();
        UIManager.Instance.ClosePopUp();
    }
}
