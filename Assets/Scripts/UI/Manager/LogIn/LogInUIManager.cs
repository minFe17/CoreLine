//using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class LogInUIManager : MonoBehaviour
{
    //private FirebaseAuth _auth;

    private bool _isLogin = false;

    public bool IsLogIn
    {
        get { return _isLogin; }
        set {  _isLogin = value; }
    }
    public void OnClickStart()
    {
        DataManager.Instance.LoadData();
        SceneManager.LoadScene("LobbyScene");
    }
    private void Start()
    {
        //_auth = FirebaseAuth.DefaultInstance;
        //
        //if (_auth.CurrentUser != null)
        //{
        //    //이미 로그인이 된 상태
        //    Debug.Log("자동 로그인: " + _auth.CurrentUser.Email);
        //    FireBaseManager.Instance.LoadGameData();
        //    UIManager.Instance.AddPanelStack(PanelStatus.StartPanel);
        //}
        //else
        //{
        //    UIManager.Instance.AddPanelStack(PanelStatus.LogInSelectPanel);
        //}
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            FireBaseManager.Instance.LogOut();
            UIManager.Instance.AddPanelStack(PanelStatus.LogInSelectPanel);
        }
    
        if (_isLogin)
        {
            OnClickStart();
        } 
    }

}