using UnityEngine;
using Utils;
using System.Collections.Generic;
using System;

public class LaboratoryPanelManager : MonoBehaviour
{
    private Dictionary<LaboratoryType, GameObject> _contents = new();
    private LaboratoryType _onType;
    private LaboratoryInformationController _information;

    private void Awake()
    {
        FindContents();
        _information = GetComponentInChildren<LaboratoryInformationController>();
    }
    private void Start()
    {
        _contents[LaboratoryType.Attack].gameObject.SetActive(false);
        _contents[LaboratoryType.Defense].gameObject.SetActive(false);
        _contents[LaboratoryType.Utility].gameObject.SetActive(false);

    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe<LaboratoryType>("ChoiceContent", OpenContent);
        EventManager.Instance.Subscribe<bool>("SettingInformation", OpenInformation);
        _information.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("ChoiceContent", (Action<LaboratoryType>)OpenContent);
        EventManager.Instance.UnSubscribe("SettingInformation", (Action<bool>)OpenInformation);
    }
    private void FindContents()
    {
        _contents[LaboratoryType.Attack]=transform.Find("NodePanel/Viewport/Attack").gameObject;
        _contents[LaboratoryType.Defense]=transform.Find("NodePanel/Viewport/Defense").gameObject;
        _contents[LaboratoryType.Utility]=transform.Find("NodePanel/Viewport/Utility").gameObject;
    }
    private void OpenContent(LaboratoryType type)
    {
        if(_onType != LaboratoryType.None)
            _contents[_onType].gameObject.SetActive(false);

        _onType = type;
        _contents[_onType].gameObject.SetActive(true);
    }
    private void OpenInformation(bool isOpen)
    {
        if (isOpen)
        {
            _information.gameObject.SetActive(true);
            EventManager.Instance.Invoke("UpdateLaboratoryInfo");
        }
        else
            _information.gameObject.SetActive(false);
    }
}
