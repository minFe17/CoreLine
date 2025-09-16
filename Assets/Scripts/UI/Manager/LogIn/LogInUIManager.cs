using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Firebase.Auth;
using Utils;
using TMPro;

public class LogInUIManager : MonoBehaviour
{
    private FirebaseAuth _auth;
    private Lobby _lobby;

    private bool _isLogin = false;
    private bool _isLoggedIn = false;

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
    private void OnDestroy()
    {
        UIManager.Instance.ClearPanel();
        UIManager.Instance.ClearPopUp();
    }

    private void Start()
    {
        _lobby = GameObject.Find("PrefabManager").GetComponent<Lobby>();
        UIManager.Instance.OpenPopUp(PopUpStatus.WaitAlret);
    }

    private void Update()
    {
        if (_lobby.IsSetting && !_isLoggedIn)
        {
            UIManager.Instance.ClosePopUp();
            LogIn();
            _isLoggedIn=true;
            MonoSingleton<AudioClipManager>.Instance.Init();
            MonoSingleton<AudioClipManager>.Instance.PlayBGM(EBGMType.UI_BGM2);
        }
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
    private void LogIn()
    {
        _auth = FirebaseAuth.DefaultInstance;

        if (_auth.CurrentUser != null)
        {
            //이미 로그인이 된 상태
            Debug.Log("자동 로그인: " + _auth.CurrentUser.Email);
            FireBaseManager.Instance.LoadGameData();
            UIManager.Instance.AddPanelStack(PanelStatus.StartPanel);
        }
        else
        {
            UIManager.Instance.AddPanelStack(PanelStatus.LogInSelectPanel);
        }
    }
}