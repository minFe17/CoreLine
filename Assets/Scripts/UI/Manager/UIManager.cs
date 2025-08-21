using UnityEngine;
using Utils;
using System.Collections.Generic;

public enum PanelStatus
{
    LobyPanel,InventoryPanel, UpgradePanel, LaboratoryPanel, PlayPanel, SettingPanel
}
public enum PopupStatus
{
    NoMoneyAlret
}
public class UIManager : SimpleSingleton<UIManager>
{
    private Stack<Panel> _panelStack = new Stack<Panel>();
    private Dictionary<PanelStatus, Panel> _panelDictionary = new Dictionary<PanelStatus, Panel>();

    public void RegisterPanel(PanelStatus status, Panel panel)
    {
        _panelDictionary[status] = panel;
    }
    public void ClearPanelStack()
    {
        _panelStack.Clear();
    }
    public void AddPanelStack(PanelStatus status)
    {
        if(_panelStack.Count!=0)
        {
            _panelStack.Peek().SwitchOffPanel();
        }
        _panelStack.Push(_panelDictionary[status]);
        _panelDictionary[status].SwitchOnPanel();
    }
    public void CloseFrontPanel()
    {
        _panelStack.Pop().SwitchOffPanel();
        if (_panelStack.Count == 0) return;
        _panelStack.Peek().SwitchOnPanel();
    }
}
