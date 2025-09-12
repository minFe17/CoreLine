using System.Collections.Generic;
using UnityEngine;
using Utils;
using static UnityEngine.Rendering.DebugUI;

public enum PanelStatus
{
    LobbyPanel,InventoryPanel, UpgradePanel, LaboratoryPanel, PlayPanel, SettingPanel,
    StorePanel,LogInSelectPanel, LogInEmailPanel,StartPanel
}
public enum PopUpStatus
{
    NoMoneyAlret, NoChoiceUnitAlret, NoChoiceStageAlret, SettingPopUp
}
public class UIManager : SimpleSingleton<UIManager>
{
    private Stack<Panel> _panelStack = new Stack<Panel>();
    private PopUp _popUp;
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
    public void UnregisterPanel(PanelStatus status)
    {
        _panelDictionary.Remove(status);
    }
    public void UnregisterPopUp(PopUpStatus status)
    {
        _popUpDictionary.Remove(status);
    }
    public void ClearPanelStack()
    {
        _panelStack.Clear();
    }
    public void AddPanelStack(PanelStatus status)
    {
        if (_panelStack.Count!=0)
        {
            if (_panelStack.Contains(_panelDictionary[status]))
            {
                var tempStack = new Stack<Panel>(new Stack<Panel>(_panelStack));
                _panelStack.Clear();

                foreach (var panel in tempStack) //이렇게 꼬일리가있나? 그나마 상점가는거정도인데 흠
                {
                    if (panel != _panelDictionary[status])
                        _panelStack.Push(panel);
                }
            }
            _panelStack.Peek().SwitchOffPanel();
        }
        _panelStack.Push(_panelDictionary[status]);
        _panelDictionary[status].SwitchOnPanel();
        if(status != PanelStatus.UpgradePanel)
            EventManager.Instance.Invoke("Reset");
    }
    public void CloseAllPanel()
    {
        for(int i=0;i< _panelStack.Count;i++)
        {
            _panelStack.Pop().SwitchOffPanel();
        }
    }
    public void CloseFrontPanel()
    {
        if (_panelStack.Peek().Status != PanelStatus.UpgradePanel)
            EventManager.Instance.Invoke("Reset");
        _panelStack.Pop().SwitchOffPanel();
        if (_panelStack.Count == 0) return;
        _panelStack.Peek().SwitchOnPanel();
    }
    public void OpenPopUp(PopUpStatus status)
    {
        if(_popUp != null)
        {
            _popUp.gameObject.SetActive(false);
        }
        _popUp = _popUpDictionary[status];
        _popUp.gameObject.SetActive(true);
    }
    public void ClosePopUp()
    {
        if (_popUp == null) return;

        _popUp.gameObject.SetActive(false);
        _popUp = null;
    }

}
