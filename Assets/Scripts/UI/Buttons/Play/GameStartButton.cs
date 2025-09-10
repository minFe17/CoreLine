using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStartButton : BaseButton
{
    private bool _isClick = false;
    protected override void OnClick()
    {
        if (_isClick) return;
        SceneManager.LoadScene("MonsterScene");
        _isClick = true;
    }
}
