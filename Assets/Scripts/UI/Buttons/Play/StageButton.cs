using UnityEngine;

public class StageButton : BaseButton
{
    private NormalStageData _data;

    public NormalStageData Data
    {
        get { return _data; }
        set { _data = value; }
    }

    protected override void OnClick()
    {
        EventManager.Instance.Invoke<NormalStageData>("SelectStage", _data);
    }
}
