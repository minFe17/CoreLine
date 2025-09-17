using UnityEngine;
using System.Collections.Generic;

public class StageButton : BaseButton
{
    private string _preStage;
    private NormalStageData _data;

    private UIColorApplier _color;
    public NormalStageData Data
    {
        get { return _data; }
        set 
        {
            _data = value;
            SettingText();
        }
    }
    public string PreStage
    {
        get { return _preStage; }
        set { _preStage = value; }
    }
    protected override void Awake()
    {
        base.Awake();
        _color = GetComponent<UIColorApplier>();
    }
    protected void Update()
    {
        if(NormalStageManager.Instance.SelectedStage.Id == _data.Id)
            _color.MyColorType = ColorType.Normal;
        else if(IsPreStageUnlocked())
            _color.MyColorType = ColorType.Light;
        else
            _color.MyColorType = ColorType.Dark;
    }
    protected void SettingText()
    {
        _buttonText.text = _data.Id;
    }
    protected override void OnClick()
    {
        base.OnClick();
        if (!IsPreStageUnlocked()) return; //ÀÌ°Å ÆË¾÷¶ç¿ì±â
        EventManager.Instance.Invoke<NormalStageData>("SelectStage", _data);
        EventManager.Instance.Invoke<bool>("IsStageSelect",true);
    }
    private bool IsPreStageUnlocked()
    {
        List<ClearStage> stage = DataManager.Instance.GameData.ClearStage;
        if (_preStage == "") return true;
        foreach (var st in stage)
        {
            if(st.StageId == _preStage) return true;
        }
        return false;
    }
}
