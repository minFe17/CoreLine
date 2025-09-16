using UnityEngine;

public class ExitButton : BaseButton
{
    protected override void OnClick()
    {
        base.OnClick();
        UIManager.Instance.CloseFrontPanel();
    }
}
