using UnityEngine;

public class StageButton : BaseButton
{
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
    protected override void Awake()
    {
        base.Awake();
        _color = GetComponent<UIColorApplier>();
    }
    protected void Update()
    {
        if(NormalStageManager.Instance.SelectedStage.Id == _data.Id)
            _color.MyColorType = ColorType.Normal;
        else
            _color.MyColorType = ColorType.Light;
    }
    protected void SettingText()
    {
        _buttonText.text = _data.Id;
    }
    protected override void OnClick()
    {
        EventManager.Instance.Invoke<NormalStageData>("SelectStage", _data);
        EventManager.Instance.Invoke<bool>("IsStageSelect",true);
    }
}
