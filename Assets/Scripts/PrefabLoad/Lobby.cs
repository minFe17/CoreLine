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
        ReadData<UnitDataList>(EDataType.UnitData);
        ReadData<FusionDataList>(EDataType.FusionUnitData);
    }

    void ReadData<T>(EDataType type) where T : new()
    {
        TextAsset data = _prefabManager.GetPrefabLoad(EPrefabType.Data).GetPrefabTextAsset(type);
        string json = data.text;
        T target = SimpleSingleton<T>.Instance;
        JsonUtility.FromJsonOverwrite(json, target);
    }
}