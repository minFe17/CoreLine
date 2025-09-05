using NUnit.Framework;
using UnityEngine;
using Utils;
using System.Collections.Generic;

public class UIGameManager : MonoSingleton<UIGameManager>
{
    private StageType _stageType = StageType.Stage1;

    public StageType StageType
    {
        get { return _stageType; }
        set 
        { 
            _stageType = value;
            EventManager.Instance.Invoke<StageType>("ChangeStage", _stageType); //색변경전용
        }
    }
    private void Awake()
    {
        DataManager.Instance.LoadData();
        
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            StageType = StageType.Stage1;
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            StageType = StageType.Stage2;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            StageType = StageType.Stage3;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            StageType = StageType.Infinity;
        }
    }

    private void Start()
    {
        //UIManager.Instance.AddPanelStack(PanelStatus.SettingPanel);
        UIManager.Instance.AddPanelStack(PanelStatus.LobyPanel);
        //UIManager.Instance.AddPanelStack(PanelStatus.InventoryPanel);
        //UIManager.Instance.AddPanelStack(PanelStatus.UpgradePanel);
        //UIManager.Instance.AddPanelStack(PanelStatus.LaboratoryPanel);
        //UIManager.Instance.AddPanelStack(PanelStatus.PlayPanel);
    }

}
