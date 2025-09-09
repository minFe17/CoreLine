using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using static SkillManager;

public class SkillSettingManager : MonoBehaviour
{
    private TextMeshProUGUI _info;
    private Dictionary<bool, PoolingManager> _buttons = new Dictionary<bool, PoolingManager>();
    //true -> setting된 풀링 false -> setting안된 풀링

    private void Awake()
    {
        _info = transform.Find("InfoBox/Info").GetComponent<TextMeshProUGUI>();
        CreateButtons();
    }
    private void Start()
    {
        SettingSkill();
       
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe<SkillSelectButton>("ChoiceSkillButton", UpdateInfoBox);
        EventManager.Instance.Subscribe<SkillSelectButton>("ChoiceSkillButton", SelectButton);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("ChoiceSkillButton", (Action<SkillSelectButton>)UpdateInfoBox);
        EventManager.Instance.UnSubscribe("ChoiceSkillButton", (Action<SkillSelectButton>)SelectButton);
    }
    private void CreateButtons()
    {
        string prefabsPath = "UI/Prefabs/Button/Play/SelectButton";
        string parentPath = "UI/PlayPanel/SkillSettingPanel/Scroll View/Viewport/Content";
        _buttons[false] = new PoolingManager(prefabsPath, parentPath);
        parentPath = "UI/PlayPanel/SkillSettingPanel/SettingSkills";
        _buttons[true] = new PoolingManager(prefabsPath, parentPath, 10);
    }
    private void SettingSkill()
    {
        //셋팅된거있으면 가져오기
        //스킬매니저에서 등록된 애들 가져오기
        IReadOnlyList<SelectedSkill> loadData = SkillManager.Instance._loadout;
        List<string> unlockedSkill = LaboratoryManager.Instance.UnlockedSkill;


        foreach (SelectedSkill skill in loadData)
        {
            SkillSelectButton btn = _buttons[true].Pop().GetComponent<SkillSelectButton>();
            btn.Data = LaboratoryManager.Instance.GetBuyLaboratoryData(skill.Id);
            btn.IsSetting = true;
        }

        foreach (string skill in unlockedSkill)
        {
            SkillSelectButton btn = _buttons[false].Pop().GetComponent<SkillSelectButton>();
            btn.Data = LaboratoryManager.Instance.GetBuyLaboratoryData(skill);
        }
    }
    private void UpdateInfoBox(SkillSelectButton data)
    {
        _info.text = data.Data.Id + "\n" + data.Data.Info + "\n수치 : " + data.Data.Effect.Value;
    }
    private void SelectButton(SkillSelectButton button)
    {
        if (button.IsSetting)
        {
            SkillSelectButton btn = _buttons[false].Pop().GetComponent<SkillSelectButton>();
            btn.Data = button.Data;
            SkillManager.Instance.RemoveAtLoadout(btn.Data);
            btn.IsSetting = false;
        }
        else
        {
            SkillSelectButton btn = _buttons[true].Pop().GetComponent<SkillSelectButton>();
            btn.Data = button.Data;
            SkillManager.Instance.AddToLoadout(btn.Data);
            btn.IsSetting = true;
        }
        button.gameObject.SetActive(false);

    }
}
