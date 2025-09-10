using UnityEngine;
using Utils;
using System.Threading.Tasks;

public class Lobby : MonoBehaviour
{
    PrefabManager _prefabManager;

    public async Task InitializeAsync()
    {
        if (_prefabManager == null)
            _prefabManager = SimpleSingleton<PrefabManager>.Instance;

        if (!_prefabManager.CheckLoadPrefab())
            await _prefabManager.LoadPrefab();

        ReadJson();
    }

    void ReadJson()
    {
        LoadData<UnitDataList>(EDataType.UnitData);
        LoadData<FusionDataList>(EDataType.FusionUnitData);
    }

    void LoadData<T>(EDataType type) where T : new()
    {
        TextAsset data = _prefabManager.GetPrefabLoad(EPrefabType.Data).GetPrefabTextAsset(type);
        string json = data.text;
        T target = SimpleSingleton<T>.Instance;
        JsonUtility.FromJsonOverwrite(json, target);
    }
}