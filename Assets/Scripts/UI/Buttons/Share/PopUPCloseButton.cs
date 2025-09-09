using UnityEngine;

public class PopUPCloseButton : BaseButton
{
    protected override void OnClick()
    {
        UIManager.Instance.ClosePopUp();
    }
}
