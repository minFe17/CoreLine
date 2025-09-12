using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class FireBaseAuthManager : SimpleSingleton<FireBaseAuthManager>
{
    private FirebaseAuth _auth;
    private FirebaseUser _user;
    private FirebaseFirestore _database;

    public FireBaseAuthManager()
    {
        _auth = FirebaseAuth.DefaultInstance;
        _database = FirebaseFirestore.DefaultInstance;
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

            ReadUserDataFromFirestore(newUser.UserId);
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

            //데이터 가져오기
        });
    }
    public void LogOut()
    {
        FirebaseAuth.DefaultInstance.SignOut();
    }
    private void ReadUserDataFromFirestore(string uid)
    {
        DocumentReference docRef = _database.Collection("UserGameData").Document(uid);

        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("Failed to read data: " + task.Exception);
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                // 데이터가 존재할 경우
                GameData data = snapshot.ConvertTo<GameData>();
                Debug.Log($"Player data Load : GuestID");
            }
            else
            {
                // 데이터가 존재하지 않을 경우
                Debug.Log("Player data not found.");
                UploadJsonToFirestore(uid);
            }
        });
    }
    private void CreateUserDataInFirestore(string uid)
    {
        GameData userData = new()
        {
         UnlockedUnit = new List<UnlockedUnit>(),
         UnlockedLaboratoryId = new List<string>(),
         PlayerMoney = 0,
         PlayerGem = 0,
         PlayerInfinityKey = 0,
         ClearStage = new List<ClearStage>(),
        };
            

        _database.Collection("UserGameData").Document(uid).SetAsync(userData).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.Log("Failed to create player data: " + task.Exception);
            }
            else
            {
                Debug.Log("Player data created successfully.");
            }
        });
    }
    private async void TestUpload(string uid)
    {
        Dictionary<string, string> userData = new Dictionary<string, string>() ;

        try
        {
            await _database.Collection("UserGameData").Document(uid).SetAsync(userData);
            Debug.Log("Player data created successfully!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to create player data: " + e);
        }
    }
    public void UploadJsonToFirestore(string uid)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("UI/Data/GameData");
        if (jsonFile == null)
        {
            Debug.LogError("JSON 파일을 찾을 수 없음!");
            return;
        }

        FirebaseGameData data = JsonConvert.DeserializeObject<FirebaseGameData>(jsonFile.text);

        try
        {
            // JSON 파싱
            JToken jsonToken = JToken.Parse(jsonFile.text);

            // Firestore에 업로드할 데이터 결정
            object firestoreData;

            if (jsonToken.Type == JTokenType.Object)
            {
                // JSON 객체 → Dictionary<string, object>
                firestoreData = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonFile.text);
            }
            else if (jsonToken.Type == JTokenType.Array)
            {
                // JSON 배열 → List<object>
                firestoreData = JsonConvert.DeserializeObject<List<object>>(jsonFile.text);
            }
            else
            {
                Debug.LogError("JSON이 객체 또는 배열 형식이 아님");
                return;
            }

            // Firestore 업로드
            _database.Collection("UserGameData").Document(uid)
                .SetAsync(firestoreData)
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        Debug.LogError("JSON 업로드 실패: " + task.Exception);
                    }
                    else
                    {
                        Debug.Log("JSON 업로드 성공! UID: " + uid);
                    }
                });
        }
        catch (Exception e)
        {
            Debug.LogError("JSON 파싱 실패: " + e);
        }
    } // 이거 수정해야됨
}
