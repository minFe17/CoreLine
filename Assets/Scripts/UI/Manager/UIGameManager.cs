using NUnit.Framework;
using UnityEngine;
using Utils;
using System.Collections.Generic;

public class UIGameManager : MonoSingleton<GameManager>
{

    private void Awake()
    {
        DataManager.Instance.LoadData();
        UnitManager.Instance.SettingData();
    }
    private void Start()
    {
        //UIManager.Instance.AddPanelStack(PanelStatus.SettingPanel);
        UIManager.Instance.AddPanelStack(PanelStatus.LobyPanel);
    }

}
