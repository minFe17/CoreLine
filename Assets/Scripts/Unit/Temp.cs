using UnityEngine;
using Utils;

public class Temp : MonoBehaviour
{
    async void Start()
    {
        if (!SimpleSingleton<PrefabManager>.Instance.CheckLoadPrefab())
            await SimpleSingleton<PrefabManager>.Instance.LoadPrefab();
        SimpleSingleton<FactoryManager>.Instance.Init();
        Debug.Log(1);
        MonoSingleton<GameStateManager>.Instance.SetState();

        CreateCamera();
        ReadJson();
    }

    private void ReadJson()
    {
        TextAsset unitData = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.Data).GetPrefabTextAsset(EDataType.UnitData);
        UnitDataList temp = SimpleSingleton<UnitDataList>.Instance;
        string data = unitData.text;
        JsonUtility.FromJsonOverwrite(data, temp);

        TextAsset fusionUnitData = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.Data).GetPrefabTextAsset(EDataType.FusionUnitData);
        FusionDataList dataList = SimpleSingleton<FusionDataList>.Instance;
        data = fusionUnitData.text;
        JsonUtility.FromJsonOverwrite(data, dataList);
    }

    void CreateCamera()
    {
        GameObject postVolume = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.Camera).GetPrefab(ECameraType.FusionPostVolume);
        GameObject fusionCamera = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.Camera).GetPrefab(ECameraType.FusionCamera);
        GameObject camera = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.Camera).GetPrefab(ECameraType.MainCamera);

        Instantiate(postVolume);
        GameObject temp = Instantiate(fusionCamera);
        MainCamera mainCamera = Instantiate(camera).GetComponent<MainCamera>();
        mainCamera.Init(temp.GetComponent<Camera>());
    }
}