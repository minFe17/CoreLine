using UnityEngine;

public class GamePlayButton : ShowPanelButton
{
    protected override void OnClick()
    {
        if(NormalStageManager.Instance.StageType != StageType.Stage1)
        {
            UIManager.Instance.OpenPopUp(PopUpStatus.WaitingNextUpdate);
            return;
        }
        base.OnClick();
    }
}
