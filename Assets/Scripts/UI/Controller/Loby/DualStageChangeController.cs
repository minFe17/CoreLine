using UnityEngine;
using System.Collections.Generic;

public class DualStageChangeController : DualButtonController<StageType>
{
    protected override void SettingList()
    {
        _list.Clear();
        _list.Add(StageType.Infinity);
        foreach(var stage in DataManager.Instance.WorldStageDatas)
        {
            _list.Add(stage.StageType);
        }
    }
   
    public override void OnClickNextButton()
    {
        base.OnClickNextButton();
        NormalStageManager.Instance.StageType = _list[_index];
    }
    public override void OnClickPrevButton()
    {
        base.OnClickPrevButton();
        NormalStageManager.Instance.StageType = _list[_index];
    }
    protected override void Awake()
    {
        MatchButtons();
    }
    protected override void OnEnable()
    {
        
    }
    protected void Start()
    {
        SettingList();
        SettingIndex();
        ChangeButtonStatus();
    }
    protected override void SettingIndex()
    {
        for (int i = 0; i < _list.Count; i++)
        {
            if (_list[i] == NormalStageManager.Instance.StageType)
            {
                _index = i;
                return;
            }
        }
    }
}
