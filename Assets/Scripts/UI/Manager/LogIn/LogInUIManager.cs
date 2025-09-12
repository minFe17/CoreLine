using Firebase.Auth;
using UnityEngine;

public class LogInUIManager : MonoBehaviour
{
    private FirebaseAuth _auth;

    private void Start()
    {
        _auth = FirebaseAuth.DefaultInstance;

        if (_auth.CurrentUser != null)
        {
            // 이미 로그인된 상태
            Debug.Log("자동 로그인: " + _auth.CurrentUser.Email);
            //UIManager.Instance.AddPanelStack(PanelStatus.); 이거 화면 터치로 바꿀까? 아님 바로 씬넘길까?
        }
        else
        {
            // 로그인 필요
            UIManager.Instance.AddPanelStack(PanelStatus.LogInSelectPanel);
        }
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            FireBaseManager.Instance.LogOut();
            UIManager.Instance.AddPanelStack(PanelStatus.LogInSelectPanel);
        }
    }
}