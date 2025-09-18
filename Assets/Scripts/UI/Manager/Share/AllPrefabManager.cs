using UnityEngine;
using UnityEngine.UIElements;
using Utils;
using Firebase.Auth;

public class AllPrefabManager : MonoBehaviour
{
    private async void Awake()
    {
        DontDestroyOnLoad(gameObject);
        DataManager.Instance.LoadData();
        Lobby lobby = GetComponent<Lobby>();
        await lobby.InitializeAsync();
    }
    private void OnApplicationQuit()
    {
        if(FirebaseAuth.DefaultInstance.CurrentUser != null)
            FireBaseManager.Instance.SaveGameData(DataManager.Instance.GameData);
    }
}
