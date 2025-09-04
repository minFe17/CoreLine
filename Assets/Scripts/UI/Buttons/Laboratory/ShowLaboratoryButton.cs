using UnityEngine;

public class ShowLaboratoryButton : BaseButton
{
    protected override void OnClick()
    {
        UIManager.Instance.AddPanelStack(PanelStatus.LaboratoryPanel);
    }
}
