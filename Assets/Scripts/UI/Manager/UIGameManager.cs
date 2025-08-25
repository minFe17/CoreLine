using NUnit.Framework;
using UnityEngine;
using Utils;
using System.Collections.Generic;

public class UIGameManager : MonoSingleton<UIGameManager>
{

    private void Awake()
    {
        DataManager.Instance.LoadData();
    }
    private void Start()
    {
        //UIManager.Instance.AddPanelStack(PanelStatus.SettingPanel);
        //UIManager.Instance.AddPanelStack(PanelStatus.LobyPanel);
        //UIManager.Instance.AddPanelStack(PanelStatus.InventoryPanel);
        UIManager.Instance.AddPanelStack(PanelStatus.UpgradePanel);
    }

}
