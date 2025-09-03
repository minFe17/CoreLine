using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LaboratoryNode : BaseButton
{
    private bool _isSelect = false;
    private bool _isUnlocked = false;
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
    protected override void OnClick()
    {
        if(_isSelect)
        {
            //패널끄기
            _isSelect = false;
            return;
        }
        _isSelect = true;
        _isUnlocked = true;
        //패널 켜기
    }

    
}
