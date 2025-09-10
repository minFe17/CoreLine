using NUnit.Framework;
using UnityEngine;
using Utils;
using System.Collections.Generic;

public class UIGameManager : MonoBehaviour
{
    private GameObject _uiPanel;

    private async void Awake()
    {
        DontDestroyOnLoad(gameObject);
        DataManager.Instance.LoadData();
        Lobby lobby = GetComponent<Lobby>();
        await lobby.InitializeAsync();
        _uiPanel = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.UI).GetPrefab(EUIPrefabType.UIPanel);
        Instantiate(_uiPanel);
    }
}
