using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class StageSettingManager : MonoBehaviour
{
    private PoolingManager _buttons;
    private TextMeshProUGUI _stageId;
    private TextMeshProUGUI _starInfo;
    private List<Image> _starImages; 
    private NormalStageData _choiceButton;

    private void Awake()
    {
        CreateButtons();
        _stageId = transform.Find("StageStatus/StageText").GetComponent<TextMeshProUGUI>();
        _starInfo = transform.Find("StarInfo").GetComponent<TextMeshProUGUI>();
        FindStars();
    }
    private void OnEnable()
    {
        SettingButtons();
    }
    private void OnDisable()
    {
        DisableButtons();
    }
    private void CreateButtons()
    {
        string prefabsPath = "UI/Prefabs/Button/Play/StageButton";
        string parentPath = "UI/PlayPanel/StagePanel/StageChoiceButtons";
        _buttons = new PoolingManager(prefabsPath, parentPath, 10);
    }
    private void SettingButtons()
    {
        StageType stage = UIGameManager.Instance.StageType;
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
    private void FindStars()
    {
        //스타 찾아주기
    }
}
