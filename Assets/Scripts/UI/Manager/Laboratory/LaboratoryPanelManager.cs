using UnityEngine;
using Utils;
using System.Collections.Generic;
using System;

public class LaboratoryPanelManager : MonoBehaviour
{
    private Dictionary<LaboratoryType, GameObject> _contents = new();
    private LaboratoryType _onType;

    private void Awake()
    {
        FindContents();
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
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("ChoiceContent", (Action<LaboratoryType>)OpenContent);
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
}
