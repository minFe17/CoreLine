using UnityEngine;

public class EmailLogInButton : BaseButton
{
    protected override void OnClick()
    {
        base.OnClick();
        UIManager.Instance.AddPanelStack(PanelStatus.LogInEmailPanel);
    }
}
