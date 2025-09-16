using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;
//using Firebase;
//using Firebase.Auth;
//using Firebase.Extensions;
//using Firebase.Database;

public class FireBaseManager : SimpleSingleton<FireBaseManager>
{
    //private string _databaseURL = "https://coreline-4f199-default-rtdb.firebaseio.com/";
    //private FirebaseAuth _auth;
    //private FirebaseUser _user;
    //private FirebaseDatabase _database;

    public FireBaseManager()
    {
        //FirebaseApp.DefaultInstance.Options.DatabaseUrl = new Uri(_databaseURL);
        //Debug.Log("Firebase 초기화 완료");
        //_auth = FirebaseAuth.DefaultInstance;
        //_database = FirebaseDatabase.DefaultInstance;
    }

    public void CreateToEmail(string email, string password)
    {
        //if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        //{
        //    UIManager.Instance.OpenPopUp(PopUpStatus.NoWriteAlret);
        //    Debug.LogWarning("이메일과 비밀번호를 입력해주세요.");
        //    return;
        //}
        //
        //_auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        //{
        //    if (task.IsCanceled)
        //    {
        //        UIManager.Instance.OpenPopUp(PopUpStatus.NoCreateAlret);
        //        Debug.LogWarning("회원가입 작업이 취소되었습니다.");
        //        return;
        //    }
        //    if (task.IsFaulted)
        //    {
        //        UIManager.Instance.OpenPopUp(PopUpStatus.NoCreateAlret);
        //        Debug.LogError("회원가입 실패: " + task.Exception);
        //        return;
        //    }
        //
        //    Firebase.Auth.AuthResult authResult = task.Result;
        //    Firebase.Auth.FirebaseUser newUser = authResult.User;
        //    Debug.Log("회원가입 성공! UID: " + newUser.UserId);
        //    SetDataBase(newUser.UserId);
        //    UIManager.Instance.OpenPopUp(PopUpStatus.SuccessCreateAlret);
        //});
    }

    public void LogInToEmail(string email, string password, Action<bool> onComplete = null)
    {
        //if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        //{
        //    UIManager.Instance.OpenPopUp(PopUpStatus.NoWriteAlret);
        //    Debug.LogWarning("이메일과 비밀번호를 입력해주세요.");
        //    return;
        //}
        //
        //_auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        //{
        //    if (task.IsCanceled)
        //    {
        //        UIManager.Instance.OpenPopUp(PopUpStatus.NoLogInAlret);
        //        Debug.LogWarning("로그인 취소");
        //        onComplete?.Invoke(false);
        //        return;
        //    }
        //
        //    if (task.IsFaulted)
        //    {
        //        UIManager.Instance.OpenPopUp(PopUpStatus.NoLogInAlret);
        //        Debug.LogError("로그인 실패: " + task.Exception);
        //        onComplete?.Invoke(false);
        //        return;
        //    }
        //
        //    Firebase.Auth.AuthResult authResult = task.Result;
        //    Firebase.Auth.FirebaseUser newUser = authResult.User;
        //    Debug.Log("로그인 성공! UID: " + newUser.UserId);
        //
        //    LoadGameData((loadedData) =>
        //    {
        //        if (loadedData != null)
        //        {
        //            DataManager.Instance.GameData = loadedData;
        //            Debug.Log("게임 데이터 로드 성공");
        //            onComplete?.Invoke(true);
        //        }
        //        else
        //        {
        //            Debug.LogWarning("게임 데이터 로드 실패");
        //            onComplete?.Invoke(false);
        //        }
        //    });
        //});
    }

    public void LogOut()
    {
        //FirebaseAuth.DefaultInstance.SignOut();
    }

    public void SaveGameData(GameData data)
    {
        // null, enum 등 포함된 객체 직렬화 설정
        //string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
        //{
        //    Formatting = Formatting.Indented,
        //    Converters = new List<JsonConverter> { new StringEnumConverter() },
        //    NullValueHandling = NullValueHandling.Include // 중요: null 포함 저장
        //});
        //
        //DatabaseReference reference = _database.GetReference("users").Child(_auth.CurrentUser.UserId);
        //reference.SetRawJsonValueAsync(json).ContinueWith(task =>
        //{
        //    if (task.IsCompletedSuccessfully)
        //        Debug.Log("게임 데이터 저장 완료");
        //    else
        //        Debug.LogWarning("게임 데이터 저장 실패: " + task.Exception);
        //});
    }

    public void LoadGameData(Action<GameData> onLoaded)
    {
        //DatabaseReference reference = _database.GetReference("users").Child(_auth.CurrentUser.UserId);
        //reference.GetValueAsync().ContinueWith(task =>
        //{
        //    if (task.IsFaulted || task.IsCanceled)
        //    {
        //        Debug.LogWarning("데이터 로드 실패: " + task.Exception);
        //        onLoaded?.Invoke(null);
        //        return;
        //    }
        //
        //    if (task.Result.Exists)
        //    {
        //        string json = task.Result.GetRawJsonValue();
        //        GameData data = JsonConvert.DeserializeObject<GameData>(json);
        //        Debug.Log("데이터 로드 성공");
        //        onLoaded?.Invoke(data);
        //    }
        //    else
        //    {
        //        Debug.LogWarning("데이터가 존재하지 않습니다.");
        //        onLoaded?.Invoke(null);
        //    }
        //});
    }

    public void LoadGameData()
    {
        //LoadGameData((loadedData) =>
        //{
        //    if (loadedData != null)
        //    {
        //        if (loadedData.UnlockedLaboratoryId == null)
        //            loadedData.UnlockedLaboratoryId = new List<string>();
        //
        //        if (loadedData.ClearStage == null)
        //            loadedData.ClearStage = new List<ClearStage>();
        //
        //        DataManager.Instance.GameData = loadedData;
        //        Debug.Log("게임 데이터 로드 및 설정 완료");
        //    }
        //    else
        //    {
        //        Debug.Log("게임 데이터 로드 실패");
        //    }
        //});
    }

    private void SetDataBase(string uid)
    {
        //GameData testData = new GameData
        //{
        //    PlayerMoney = 0,
        //    PlayerGem = 0,
        //    PlayerInfinityKey = 0,
        //
        //    UnlockedUnit = new List<UnlockedUnit>
        //    {
        //        new UnlockedUnit
        //        {
        //            UnitType = EUnitType.Archer,
        //            AttackDamageLevel = 1,
        //            HealthPointLevel = 1,
        //            AttackRangeLevel = 1,
        //            AttackSpeedLevel = 1
        //        }
        //    },
        //
        //    UnlockedLaboratoryId = new List<string>(),
        //
        //    ClearStage = new List<ClearStage>()
        //};
        //
        //string json = JsonConvert.SerializeObject(testData, new JsonSerializerSettings
        //{
        //    Formatting = Formatting.Indented,
        //    Converters = new List<JsonConverter> { new StringEnumConverter() }
        //});
        //
        //DatabaseReference reference = _database.GetReference("users").Child(uid);
        //reference.SetRawJsonValueAsync(json).ContinueWith(task =>
        //{
        //    if (task.IsCompleted)
        //    {
        //        Debug.Log("GameData 초기화 및 업로드 성공");
        //    }
        //    else
        //    {
        //        Debug.LogWarning("GameData 업로드 실패: " + task.Exception);
        //    }
        //});
    }
}
