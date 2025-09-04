using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class LaboratoryNode : BaseButton
{
    private bool _isUnlocked = false;
    private bool _isSelect = false;
    private List<LaboratoryNode> _parents = new List<LaboratoryNode>();
    private Image _icon;
    private LaboratoryData _data;

    public LaboratoryData Data
    {
        get { return _data; }
        set { _data = value; }
    }
    public bool IsUnlocked
    {
        get { return _isUnlocked; }
        set { _isUnlocked = value; }
    }
    
    public void AddParent(LaboratoryNode node)
    {
        _parents.Add(node);
    }
    protected override void Awake()
    {
        base.Awake();
        _icon = transform.Find("Icon").GetComponent<Image>();
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe<LaboratoryData>("BuyLaboratory", BuyLaboratory);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("BuyLaboratory", (Action<LaboratoryData>)BuyLaboratory);
    }
    protected override void OnClick()
    {
        if (!CheckParent())
        {
            EventManager.Instance.Invoke<LaboratoryData>("ChangeChoiceLaboratory", Data);
            EventManager.Instance.Invoke<bool>("SettingInformation", false);
            return;
        }
        if (LaboratoryManager.Instance.ChoiceLaboratory.Id == _data.Id && _isSelect == true)
        {
            EventManager.Instance.Invoke<bool>("SettingInformation", false);
            _isSelect = false;
            return;
        }

        EventManager.Instance.Invoke<LaboratoryData>("ChangeChoiceLaboratory", Data);
        EventManager.Instance.Invoke<bool>("SettingInformation", true);
        _isSelect = true;

        //패널 켜기
    }

    private void BuyLaboratory(LaboratoryData data)
    {
        if (data.Id != _data.Id) return;
        _isUnlocked = true;
    }

    private bool CheckParent()
    {
        foreach(LaboratoryNode node in _parents)
        {
            if (node.IsUnlocked) continue;
            else
                return false;
        }

        return true;
    }
    
}
