using System.Collections.Generic;
using UnityEngine;
using Utils;
using static UnityEngine.Rendering.DebugUI;

public enum PanelStatus
{
    LobyPanel,InventoryPanel, UpgradePanel, LaboratoryPanel, PlayPanel, SettingPanel,
    StorePanel
}
public enum PopUpStatus
{
    NoMoneyAlret, CheckBuyPopup
}
public class UIManager : SimpleSingleton<UIManager>
{
    private Stack<Panel> _panelStack = new Stack<Panel>();
    private Dictionary<PanelStatus, Panel> _panelDictionary = new Dictionary<PanelStatus, Panel>();
    private Dictionary<PopUpStatus, PopUp> _popUpDictionary = new Dictionary<PopUpStatus, PopUp>();

    public void RegisterPanel(PanelStatus status, Panel panel)
    {
        _panelDictionary[status] = panel;
    }
    public void RegisterPopUp(PopUpStatus status , PopUp popUP)
    {
        _popUpDictionary[status] = popUP;
    }
    public void ClearPanelStack()
    {
        _panelStack.Clear();
    }
    public void AddPanelStack(PanelStatus status)
    {
        if (_panelStack.Count!=0)
        {
            _panelStack.Peek().SwitchOffPanel();
        }
        _panelStack.Push(_panelDictionary[status]);
        _panelDictionary[status].SwitchOnPanel();
        if(status != PanelStatus.UpgradePanel)
            EventManager.Instance.Invoke("Reset");
    }
    public void CloseFrontPanel()
    {
        if (_panelStack.Peek().Status != PanelStatus.UpgradePanel)
            EventManager.Instance.Invoke("Reset");
        _panelStack.Pop().SwitchOffPanel();
        if (_panelStack.Count == 0) return;
        _panelStack.Peek().SwitchOnPanel();
    }
}
