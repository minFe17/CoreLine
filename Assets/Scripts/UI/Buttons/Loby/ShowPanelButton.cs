using UnityEngine;

public class ShowPanelButton : BaseButton
{
    [SerializeField]
    private PanelStatus _status;
    protected override void OnClick()
    {
        UIManager.Instance.AddPanelStack(_status);
    }
}
