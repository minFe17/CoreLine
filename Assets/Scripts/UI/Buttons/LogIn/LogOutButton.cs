using UnityEngine;

public class LogOutButton : BaseButton
{
    protected override void OnClick()
    {
        base.OnClick();
        FireBaseManager.Instance.LogOut();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
