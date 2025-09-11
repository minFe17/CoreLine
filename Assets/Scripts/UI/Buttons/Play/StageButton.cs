using UnityEngine;

public class StageButton : BaseButton
{
    private NormalStageData _data;

    public NormalStageData Data
    {
        get { return _data; }
        set 
        {
            _data = value;
            SettingText();
        }
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
