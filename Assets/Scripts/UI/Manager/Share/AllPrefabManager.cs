using UnityEngine;
using UnityEngine.UIElements;
using Utils;

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
        FireBaseManager.Instance.SaveGameData(DataManager.Instance.GameData);
    }
}
