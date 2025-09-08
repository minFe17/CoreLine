using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System;

public class StageSettingManager : MonoBehaviour
{
    private PoolingManager _buttons;
    private TextMeshProUGUI _stageId;
    private TextMeshProUGUI _starInfo;
    private NormalStageData _choiceButton;

    private void Awake()
    {
        CreateButtons();
        _stageId = transform.Find("StageStatus/StageText").GetComponent<TextMeshProUGUI>();
        _starInfo = transform.Find("StarInfo").GetComponent<TextMeshProUGUI>();
    }
    private void OnEnable()
    {
        DataManager.Instance.LoadData();
        SettingButtons();
        EventManager.Instance.Subscribe<NormalStageData>("SelectStage", UpdateText);
    }
    private void OnDisable()
    {
        DisableButtons();
        EventManager.Instance.UnSubscribe("SelectStage", (Action<NormalStageData>)UpdateText);
    }
    private void CreateButtons()
    {
        string prefabsPath = "UI/Prefabs/Button/Play/StageButton";
        string parentPath = "UI/PlayPanel/StagePanel/StageChoiceButtons";
        _buttons = new PoolingManager(prefabsPath, parentPath, 10);
    }
    private void SettingButtons()
    {
        StageType stage = NormalStageManager.Instance.StageType;
        List<NormalStageData> stages = DataManager.Instance.GetStages(stage);

        foreach (var stageData in stages)
        {
            StageButton btn = _buttons.Pop().GetComponent<StageButton>();
            btn.Data = stageData;
        }
    }
    private void DisableButtons()
    {
        List<GameObject> buttons = _buttons.GetAllToActiveTrue();
        foreach (var button in buttons)
        {
            button.gameObject.SetActive(false);
        }
    }

    private void UpdateText(NormalStageData data)
    {
        _stageId.text = data.Name;
        List<Condition> condition = data.Condition;
        string text="";
        foreach (var conditionData in condition)
        {
            text += conditionData.Info + "\n";
        }
        _starInfo.text = text;
    }
}
