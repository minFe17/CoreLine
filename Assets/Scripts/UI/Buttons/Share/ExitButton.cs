using UnityEngine;

public class ExitButton : BaseButton
{
    protected override void OnClick()
    {
        UIManager.Instance.CloseFrontPanel();
    }
}
