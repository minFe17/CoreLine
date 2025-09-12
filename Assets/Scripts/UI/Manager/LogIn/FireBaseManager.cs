//using Firebase.Auth;
//using Firebase.Extensions;
//using Firebase.Database;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;
using Utils;

public class FireBaseManager : SimpleSingleton<FireBaseManager>
{
   // private FirebaseAuth _auth;
   // private FirebaseUser _user;
   // private DatabaseReference _database;

    public FireBaseManager()
    {
        //_auth = FirebaseAuth.DefaultInstance;
        //_database = DatabaseReference;
    }

    public void CreateToEmail(string email, string password)
    {

       // if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
       // {
       //     Debug.LogWarning("이메일과 비밀번호를 입력하세요.");
       //     return;
       // }
       //
       // _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
       // {
       //     if (task.IsCanceled)
       //     {
       //         Debug.LogWarning("회원가입 작업이 취소되었습니다.");
       //         return;
       //     }
       //     if (task.IsFaulted)
       //     {
       //         Debug.LogError("회원가입 실패: " + task.Exception);
       //         return;
       //     }
       //
       //     Firebase.Auth.AuthResult authResult = task.Result;
       //     Firebase.Auth.FirebaseUser newUser = authResult.User;
       //     Debug.Log("회원가입 성공! UID: " + newUser.UserId);
       //
       //     
       // });
    }
    public void LogInToEmail(string email, string password)
    {
       // if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
       // {
       //     Debug.LogWarning("이메일과 비밀번호를 입력하세요.");
       //     return;
       // }
       //
       // _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
       // {
       //     if (task.IsCanceled)
       //     {
       //         Debug.LogWarning("로그인 취소");
       //         return;
       //     }
       //     if (task.IsFaulted)
       //     {
       //         Debug.LogError("로그인 실패: " + task.Exception);
       //         return;
       //     }
       //
       //     Firebase.Auth.AuthResult authResult = task.Result;
       //     Firebase.Auth.FirebaseUser newUser = authResult.User;
       //     Debug.Log("로그인 성공! UID: " + newUser.UserId);
       //
       //     //데이터 가져오기
       // });
    }
    public void LogOut()
    {
       // FirebaseAuth.DefaultInstance.SignOut();
    }

}
