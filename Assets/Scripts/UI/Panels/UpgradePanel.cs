using UnityEngine;

public class UpgradePanel : Panel
{
    protected override void RegisterPanelStatus()
    {
        _status = PanelStatus.UpgradePanel;
    }
}
