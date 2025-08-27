using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UIPrefabLoad : PrefabLoadBase
{
    Dictionary<EUIPrefabType, GameObject> _unitDict = new Dictionary<EUIPrefabType, GameObject>();

    public override async Task LoadPrefab()
    {
        if (_addressableManager == null)
            Init();

        for (int i = 0; i < (int)EUIPrefabType.Max; i++)
        {
            GameObject prefab = await _addressableManager.GetAddressableAsset<GameObject>($"{(EUIPrefabType)i}");
            if (prefab != null && !_unitDict.ContainsKey((EUIPrefabType)i))
                _unitDict.Add((EUIPrefabType)i, prefab);
        }
    }

    public override GameObject GetPrefab<TEnum>(TEnum type)
    {
        EUIPrefabType key = (EUIPrefabType)(object)type;
        return _unitDict[key];
    }
}