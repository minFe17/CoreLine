using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Utils;

public class UIGameManager : MonoBehaviour
{
    private GameObject _uiPanel;
    private GameObject _unitAnimation;

    private async void Awake()
    {
        DontDestroyOnLoad(gameObject);
        DataManager.Instance.LoadData();
        Lobby lobby = GetComponent<Lobby>();
        await lobby.InitializeAsync();
        _uiPanel = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.UI).GetPrefab(EUIPrefabType.UIPanel);
        _unitAnimation = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.UI).GetPrefab(EUIPrefabType.UnitAnimations);
        Instantiate(_uiPanel);
        Instantiate(_unitAnimation);
    }
    private void OnApplicationQuit()
    {
        FireBaseManager.Instance.SaveGameData(DataManager.Instance.GameData);
    }

}
