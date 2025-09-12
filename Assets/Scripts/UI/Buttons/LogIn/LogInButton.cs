//using TMPro;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//
//public class LogInButton : BaseButton
//{
//    [SerializeField]
//    private TMP_InputField _email;
//    [SerializeField]
//    private TMP_InputField _password;
//
//
//    private LogInUIManager _loginManager;
//
//    private void Start()
//    {
//        _loginManager = GameObject.Find("LogInUI").GetComponent<LogInUIManager>();
//    }
//    protected override void OnClick()
//    {
//        FireBaseManager.Instance.LogInToEmail(_email.text, _password.text, (success) =>
//        {
//            if (success)
//            {
//                _loginManager.IsLogIn = true;
//            }
//            else
//            {
//                Debug.LogWarning("로그인 또는 데이터 로딩 실패");
//            }
//        });
//    }
//}
