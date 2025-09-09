using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class GameStart : MonoBehaviour
{
    PrefabManager _prefabManager;
    async void Start()
    {
        if(_prefabManager == null)
            _prefabManager = SimpleSingleton<PrefabManager>.Instance;
        // 임시 (로비 연결 후 삭제)
        //-------
        if (!_prefabManager.CheckLoadPrefab())
            await _prefabManager.LoadPrefab();
        //-------

        SimpleSingleton<FactoryManager>.Instance.Init();
        MonoSingleton<GameStateManager>.Instance.SetState();

        CreateCamera();
        CreateUI();

        // 임시
        ReadJson();
    }

    // 임시
    void ReadJson()
    {
        LoadData<UnitDataList>(EDataType.UnitData);
        LoadData<FusionDataList>(EDataType.FusionUnitData);
    }

    // 임시
    void LoadData<T>(EDataType type) where T : new()
    {
        TextAsset data = _prefabManager.GetPrefabLoad(EPrefabType.Data).GetPrefabTextAsset(type);
        string json = data.text;
        T target = SimpleSingleton<T>.Instance;
        JsonUtility.FromJsonOverwrite(json, target);
    }

    private void CreateUI()
    {
        GameObject prefab = _prefabManager.GetPrefabLoad(EPrefabType.UI).GetPrefab(EUIPrefabType.InGameUI);
        Instantiate(prefab);
    }

    void CreateCamera()
    {
        List<GameObject> cameraList = new List<GameObject>();
        for(int i=0; i<(int)ECameraType.Max; i++)
            cameraList.Add(CreateCamera((ECameraType)i));

        Camera fusionCamera = cameraList[(int)ECameraType.FusionCamera].GetComponent<Camera>();
        MainCamera mainCamera = cameraList[(int)ECameraType.MainCamera].GetComponent<MainCamera>();
        mainCamera.Init(fusionCamera);
        GetComponent<BuildableHighlighter>().SetCamera(mainCamera.GetComponent<Camera>());
    }

    GameObject CreateCamera(ECameraType type)
    {
        GameObject prefab = _prefabManager.GetPrefabLoad(EPrefabType.Camera).GetPrefab(type);
        return Instantiate(prefab);
    }
}
