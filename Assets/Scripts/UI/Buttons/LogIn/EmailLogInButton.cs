using UnityEngine;

public class EmailLogInButton : BaseButton
{
    protected override void OnClick()
    {
        UIManager.Instance.AddPanelStack(PanelStatus.LogInEmailPanel);
    }
}
