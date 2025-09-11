using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameStartButton : BaseButton
{
    private bool _isSelect = false;
    private bool _isClick = false;

    private void OnEnable()
    {
        EventManager.Instance.Subscribe<bool>("IsStageSelect", IsSelect);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("IsStageSelect", (Action<bool>)IsSelect);
    }
    protected override void OnClick()
    {
        if(NormalStageManager.Instance.StageType==StageType.Infinity)
        {
            //SceneManager.LoadScene("MonsterScene");
            //무한의 탑으로 넘기기
            return;
        }
        else if (!_isSelect)
        {
            UIManager.Instance.OpenPopUp(PopUpStatus.NoChoiceStageAlret);
            return;
        }
        if (_isClick) return;


        SceneManager.LoadScene("MonsterScene");
        _isClick = true;
    }
    private void IsSelect(bool isSelect)
    {
        _isSelect = isSelect;
    }
}
