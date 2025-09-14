//using Firebase.Auth;
//using Firebase.Extensions;
//using Firebase.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;
//using Firebase;
//using Newtonsoft.Json.Converters;
//using UnityEngine.SceneManagement;

public class FireBaseManager : SimpleSingleton<FireBaseManager>
{
   // private string _databaseURL = "https://coreline-4f199-default-rtdb.firebaseio.com/";
   // private FirebaseAuth _auth;
   // private FirebaseUser _user;
   // private FirebaseDatabase _database;

    public FireBaseManager()
    {
    //    FirebaseApp.DefaultInstance.Options.DatabaseUrl = new Uri(_databaseURL);
    //    Debug.Log("설정됨");
    //    _auth = FirebaseAuth.DefaultInstance;
    //    _database = FirebaseDatabase.DefaultInstance;
    }

    public void CreateToEmail(string email, string password)
    {
    //
    //    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
    //    {
    //        Debug.LogWarning("이메일과 비밀번호를 입력하세요.");
    //        return;
    //    }
    //   
    //    _auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
    //    {
    //        if (task.IsCanceled)
    //        {
    //            Debug.LogWarning("회원가입 작업이 취소되었습니다.");
    //            return;
    //        }
    //        if (task.IsFaulted)
    //        {
    //            Debug.LogError("회원가입 실패: " + task.Exception);
    //            return;
    //        }
    //   
    //        Firebase.Auth.AuthResult authResult = task.Result;
    //        Firebase.Auth.FirebaseUser newUser = authResult.User;
    //        Debug.Log("회원가입 성공! UID: " + newUser.UserId);
    //        SetDataBase(newUser.UserId);
    //
    //    });
    }
    public void LogInToEmail(string email, string password, Action<bool> onComplete = null)
    {//멀티스레드 이용하는중
    //    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
    //    {
    //        Debug.LogWarning("이메일과 비밀번호를 입력하세요.");
    //        return;
    //    }
    //
    //    _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
    //    {
    //        if (task.IsCanceled)
    //        {
    //            Debug.LogWarning("로그인 취소");
    //            onComplete?.Invoke(false);
    //            return;
    //        }
    //
    //        if (task.IsFaulted)
    //        {
    //            Debug.LogError("로그인 실패: " + task.Exception);
    //            onComplete?.Invoke(false);
    //            return;
    //        }
    //
    //        Firebase.Auth.AuthResult authResult = task.Result;
    //        Firebase.Auth.FirebaseUser newUser = authResult.User;
    //        Debug.Log("로그인 성공! UID: " + newUser.UserId);
    //
    //        LoadGameData((loadedData) =>
    //        {
    //            if (loadedData != null)
    //            {
    //                DataManager.Instance.GameData = loadedData;
    //                Debug.Log("데이터 로드 성공");
    //                onComplete?.Invoke(true); // 여기서 씬 이동 호출 가능
    //            }
    //            else
    //            {
    //                Debug.LogWarning("데이터 로드 실패");
    //                onComplete?.Invoke(false);
    //            }
    //        });
    //    });
    }

    public void LogOut()
    {
     //   FirebaseAuth.DefaultInstance.SignOut();

    }
    public void SaveGameData(GameData data)
    {
    //    // null, enum 등을 포함한 전체 데이터 직렬화
    //    string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
    //    {
    //        Formatting = Formatting.Indented,
    //        Converters = new List<JsonConverter> { new StringEnumConverter() },
    //        NullValueHandling = NullValueHandling.Include // 중요: null 값도 포함
    //    });
    //
    //    DatabaseReference reference = _database.GetReference("users").Child(_auth.CurrentUser.UserId);
    //    reference.SetRawJsonValueAsync(json).ContinueWith(task =>
    //    {
    //        if (task.IsCompletedSuccessfully)
    //            Debug.Log("세이브 완료");
    //        else
    //            Debug.LogWarning("세이브 실패: " + task.Exception);
    //    });
    }
    public void LoadGameData( Action<GameData> onLoaded)
    {
      //  DatabaseReference reference = _database.GetReference("users").Child(_auth.CurrentUser.UserId);
      //  reference.GetValueAsync().ContinueWith(task =>
      //  {
      //      if (task.IsFaulted || task.IsCanceled)
      //      {
      //          Debug.LogWarning("로드 실패: " + task.Exception);
      //          onLoaded?.Invoke(null);
      //          return;
      //      }
      //
      //      if (task.Result.Exists)
      //      {
      //          string json = task.Result.GetRawJsonValue();
      //          GameData data = JsonConvert.DeserializeObject<GameData>(json);
      //          Debug.Log("로드 완료");
      //          onLoaded?.Invoke(data);
      //      }
      //      else
      //      {
      //          Debug.LogWarning("데이터가 없습니다.");
      //          onLoaded?.Invoke(null);
      //      }
      //  });
    }
    public void LoadGameData()
    {
       // LoadGameData((loadedData) =>
       // {
       //     if (loadedData != null)
       //     {
       //         if (loadedData.UnlockedLaboratoryId == null)
       //             loadedData.UnlockedLaboratoryId = new List<string>();
       //
       //         if (loadedData.ClearStage == null)
       //             loadedData.ClearStage = new List<ClearStage>();
       //         DataManager.Instance.GameData = loadedData;
       //
       //         Debug.Log("데이터 셋팅완료~");
       //     }
       //     else
       //     {
       //         Debug.Log("데이터 로드 실패");
       //     }
       // });
    }
    //private void SetDataBase(string uid)
    //{
    //    GameData testData = new GameData
    //    {
    //        PlayerMoney = 0,
    //        PlayerGem = 0,
    //        PlayerInfinityKey = 0,
    //
    //        UnlockedUnit = new List<UnlockedUnit>
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
    //        UnlockedLaboratoryId = new List<string>(),
    //
    //        ClearStage = new List<ClearStage>
    //    {
    //    }
    //    };
    //
    //    string json = JsonConvert.SerializeObject(testData, new JsonSerializerSettings
    //    {
    //        Formatting = Formatting.Indented,
    //        Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() }
    //    });
    //
    //    DatabaseReference reference = _database.GetReference("users").Child(uid);
    //    reference.SetRawJsonValueAsync(json).ContinueWith(task =>
    //    {
    //        if (task.IsCompleted)
    //        {
    //            Debug.Log("GameData 초기화 및 업로드 완료");
    //        }
    //        else
    //        {
    //            Debug.LogWarning("업로드 실패: " + task.Exception);
    //        }
    //    });
    //}

}
