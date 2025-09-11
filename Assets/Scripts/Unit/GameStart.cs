using System.Collections.Generic;
using UnityEngine;
using Utils;

public class GameStart : MonoBehaviour
{
    PrefabManager _prefabManager;

    void Start()
    {
        if(_prefabManager == null)
            _prefabManager = SimpleSingleton<PrefabManager>.Instance;

        SimpleSingleton<FactoryManager>.Instance.Init();
        MonoSingleton<GameStateManager>.Instance.SetState();

        CreateCamera();
        CreateUI();
        MonoSingleton<AudioClipManager>.Instance.Init();
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