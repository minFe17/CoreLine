using UnityEngine;

public class ShowPanelButton : BaseButton
{
    [SerializeField]
    protected PanelStatus _status;
    protected override void OnClick()
    {
        base.OnClick();
        UIManager.Instance.AddPanelStack(_status);
    }
}
