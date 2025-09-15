//using Firebase.Auth;
//using Firebase.Extensions;
//using Firebase.Database;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
//using Newtonsoft.Json;
using System;
//using System.Collections.Generic;
//using UnityEngine;
using Utils;
//using Firebase;
//using Newtonsoft.Json.Converters;

public class FireBaseManager : SimpleSingleton<FireBaseManager>
{
    //private string _databaseURL = "https://coreline-4f199-default-rtdb.firebaseio.com/";
    //private FirebaseAuth _auth;
    //private FirebaseUser _user;
    //private FirebaseDatabase _database;

    //public FireBaseManager()
    //{
    //    FirebaseApp.DefaultInstance.Options.DatabaseUrl = new Uri(_databaseURL);
    //    Debug.Log("������");
    //    _auth = FirebaseAuth.DefaultInstance;
    //    _database = FirebaseDatabase.DefaultInstance;
    //}

    public void CreateToEmail(string email, string password)
    {
    
        //if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        //{
        //    Debug.LogWarning("�̸��ϰ� ��й�ȣ�� �Է��ϼ���.");
        //    return;
        //}
        //
        //_auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
        //{
        //    if (task.IsCanceled)
        //    {
        //        Debug.LogWarning("ȸ������ �۾��� ��ҵǾ����ϴ�.");
        //        return;
        //    }
        //    if (task.IsFaulted)
        //    {
        //        Debug.LogError("ȸ������ ����: " + task.Exception);
        //        return;
        //    }
        //
        //    Firebase.Auth.AuthResult authResult = task.Result;
        //    Firebase.Auth.FirebaseUser newUser = authResult.User;
        //    Debug.Log("ȸ������ ����! UID: " + newUser.UserId);
        //    SetDataBase(newUser.UserId);
        //
        //});
    }
    public void LogInToEmail(string email, string password, Action<bool> onComplete = null)
    {//��Ƽ������ �̿��ϴ���
       // if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
       // {
       //     Debug.LogWarning("�̸��ϰ� ��й�ȣ�� �Է��ϼ���.");
       //     return;
       // }
       //
       // _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
       // {
       //     if (task.IsCanceled)
       //     {
       //         Debug.LogWarning("�α��� ���");
       //         onComplete?.Invoke(false);
       //         return;
       //     }
       //
       //     if (task.IsFaulted)
       //     {
       //         Debug.LogError("�α��� ����: " + task.Exception);
       //         onComplete?.Invoke(false);
       //         return;
       //     }
       //
       //     Firebase.Auth.AuthResult authResult = task.Result;
       //     Firebase.Auth.FirebaseUser newUser = authResult.User;
       //     Debug.Log("�α��� ����! UID: " + newUser.UserId);
       //
       //     LoadGameData((loadedData) =>
       //     {
       //         if (loadedData != null)
       //         {
       //             DataManager.Instance.GameData = loadedData;
       //             Debug.Log("������ �ε� ����");
       //             onComplete?.Invoke(true); // ���⼭ �� �̵� ȣ�� ����
       //         }
       //         else
       //         {
       //             Debug.LogWarning("������ �ε� ����");
       //             onComplete?.Invoke(false);
       //         }
       //     });
       // });
    }

    public void LogOut()
    {
        //FirebaseAuth.DefaultInstance.SignOut();

    }
    public void SaveGameData(GameData data)
    {
        // null, enum ���� ������ ��ü ������ ����ȭ
       // string json = JsonConvert.SerializeObject(data, new JsonSerializerSettings
       // {
       //     Formatting = Formatting.Indented,
       //     Converters = new List<JsonConverter> { new StringEnumConverter() },
       //     NullValueHandling = NullValueHandling.Include // �߿�: null ���� ����
       // });
       //
       // DatabaseReference reference = _database.GetReference("users").Child(_auth.CurrentUser.UserId);
       // reference.SetRawJsonValueAsync(json).ContinueWith(task =>
       // {
       //     if (task.IsCompletedSuccessfully)
       //         Debug.Log("���̺� �Ϸ�");
       //     else
       //         Debug.LogWarning("���̺� ����: " + task.Exception);
       // });
    }
    public void LoadGameData( Action<GameData> onLoaded)
    {
       // DatabaseReference reference = _database.GetReference("users").Child(_auth.CurrentUser.UserId);
       // reference.GetValueAsync().ContinueWith(task =>
       // {
       //     if (task.IsFaulted || task.IsCanceled)
       //     {
       //         Debug.LogWarning("�ε� ����: " + task.Exception);
       //         onLoaded?.Invoke(null);
       //         return;
       //     }
       //
       //     if (task.Result.Exists)
       //     {
       //         string json = task.Result.GetRawJsonValue();
       //         GameData data = JsonConvert.DeserializeObject<GameData>(json);
       //         Debug.Log("�ε� �Ϸ�");
       //         onLoaded?.Invoke(data);
       //     }
       //     else
       //     {
       //         Debug.LogWarning("�����Ͱ� �����ϴ�.");
       //         onLoaded?.Invoke(null);
       //     }
       // });
    }
    public void LoadGameData()
    {
      //  LoadGameData((loadedData) =>
      //  {
      //      if (loadedData != null)
      //      {
      //          if (loadedData.UnlockedLaboratoryId == null)
      //              loadedData.UnlockedLaboratoryId = new List<string>();
      // 
      //          if (loadedData.ClearStage == null)
      //              loadedData.ClearStage = new List<ClearStage>();
      //          DataManager.Instance.GameData = loadedData;
      // 
      //          Debug.Log("������ ���ÿϷ�~");
      //      }
      //      else
      //      {
      //          Debug.Log("������ �ε� ����");
      //      }
      //  });
    }
    private void SetDataBase(string uid)
    {
     //   GameData testData = new GameData
     //   {
     //       PlayerMoney = 0,
     //       PlayerGem = 0,
     //       PlayerInfinityKey = 0,
     //
     //       UnlockedUnit = new List<UnlockedUnit>
     //   {
     //       new UnlockedUnit
     //       {
     //           UnitType = EUnitType.Archer,
     //           AttackDamageLevel = 1,
     //           HealthPointLevel = 1,
     //           AttackRangeLevel = 1,
     //           AttackSpeedLevel = 1
     //       }
     //   },
     //
     //       UnlockedLaboratoryId = new List<string>(),
     //
     //       ClearStage = new List<ClearStage>
     //   {
     //   }
     //   };
     //
     //   string json = JsonConvert.SerializeObject(testData, new JsonSerializerSettings
     //   {
     //       Formatting = Formatting.Indented,
     //       Converters = new List<JsonConverter> { new Newtonsoft.Json.Converters.StringEnumConverter() }
     //   });
     //
     //   DatabaseReference reference = _database.GetReference("users").Child(uid);
     //   reference.SetRawJsonValueAsync(json).ContinueWith(task =>
     //   {
     //       if (task.IsCompleted)
     //       {
     //           Debug.Log("GameData �ʱ�ȭ �� ���ε� �Ϸ�");
     //       }
     //       else
     //       {
     //           Debug.LogWarning("���ε� ����: " + task.Exception);
     //       }
     //   });
    }

}
