using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CameraPrefabLoad : PrefabLoadBase
{
    Dictionary<ECameraType, GameObject> _cameraDict = new Dictionary<ECameraType, GameObject>();

    public override async Task LoadPrefab()
    {
        if (_addressableManager == null)
            Init();

        for (int i = 0; i < (int)ECameraType.Max; i++)
        {
            GameObject prefab = await _addressableManager.GetAddressableAsset<GameObject>($"{(ECameraType)i}");
            if (prefab != null && !_cameraDict.ContainsKey((ECameraType)i))
                _cameraDict.Add((ECameraType)i, prefab);
        }
    }

    public override GameObject GetPrefab<TEnum>(TEnum type)
    {
        ECameraType key = (ECameraType)(object)type;
        return _cameraDict[key];
    }
}