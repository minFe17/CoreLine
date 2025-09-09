using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;

public class LaboratoryNode : BaseButton
{
    private bool _isUnlocked = false;
    private bool _isSelect = false;
    private List<LaboratoryNode> _parents = new List<LaboratoryNode>();
    private Image _icon;
    private LaboratoryData _data;
    private UIColorApplier _color;

    public LaboratoryData Data
    {
        get { return _data; }
        set 
        { 
            _data = value;
            CheckUnlocked();
            ChangeIcon();
        }
    }
    public bool IsUnlocked
    {
        get { return _isUnlocked; }
        set 
        {
            _isUnlocked = value;
            ChangeColor();
        }
    }
    
    public void AddParent(LaboratoryNode node)
    {
        _parents.Add(node);
    }
    protected override void Awake()
    {
        base.Awake();
        _icon = transform.Find("Icon").GetComponent<Image>();
        _color = GetComponent<UIColorApplier>();
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe<LaboratoryData>("BuyLaboratory", BuyLaboratory);
        ChangeColor();
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("BuyLaboratory", (Action<LaboratoryData>)BuyLaboratory);
    }
    private void CheckUnlocked()
    {
        List<string> unlocked = DataManager.Instance.GameData.UnlockedLaboratoryId;
        
        if(unlocked.Contains(_data.Id))
            _isUnlocked = true;
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
    private void ChangeIcon()
    {
        string path = "UI/Image/Icon/";
        switch(_data.Effect.TargetStatus)
        {
            case TargetStatus.Skill:
                path = "Skills/" + _data.Id;
                break;
            default:
                path += _data.Effect.TargetStatus.ToString();
                break;
        }
        _icon.sprite = Resources.Load<Sprite>(path);
    }
    private void ChangeColor()
    {
        if (_isUnlocked)
        {
            _color.MyColorType = ColorType.Light;
        }
        else
            _color.MyColorType = ColorType.Dark;
    }
    private void BuyLaboratory(LaboratoryData data)
    {
        if (data.Id != _data.Id) return;
        _isUnlocked = true;
        ChangeColor();
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
