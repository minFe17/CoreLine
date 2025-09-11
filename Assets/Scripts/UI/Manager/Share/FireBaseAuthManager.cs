using UnityEngine;
using Firebase.Auth;
using UnityEngine.UI;
using TMPro;
using Utils;

public class FireBaseAuthManager : SimpleSingleton<FireBaseAuthManager>
{
    private FirebaseAuth _auth;
    private FirebaseUser _user;

    public FireBaseAuthManager()
    {
        _auth = FirebaseAuth.DefaultInstance;
    }

    public void CreateToEmail(string email, string password)
    {

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("이메일과 비밀번호를 입력하세요.");
            return;
        }

        _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogWarning("회원가입 작업이 취소되었습니다.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("회원가입 실패: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult authResult = task.Result;
            Firebase.Auth.FirebaseUser newUser = authResult.User;
            Debug.Log("회원가입 성공! UID: " + newUser.UserId);
        });
    }
    public void LogInToEmail(string email, string password)
    {
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.LogWarning("이메일과 비밀번호를 입력하세요.");
            return;
        }

        _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogWarning("로그인 취소");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("로그인 실패: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult authResult = task.Result;
            Firebase.Auth.FirebaseUser newUser = authResult.User;
            Debug.Log("로그인 성공! UID: " + newUser.UserId);
        });
    }
    public void LogOut()
    {
        FirebaseAuth.DefaultInstance.SignOut();
    }
    public void SignInWithGoogle(string idToken, string accessToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, accessToken);
        _auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Firebase 로그인 실패: " + task.Exception);
                return;
            }

            FirebaseUser newUser = task.Result;
            Debug.LogFormat("Firebase 로그인 성공! User: {0} ({1})", newUser.DisplayName, newUser.UserId);
        });
    }

}
