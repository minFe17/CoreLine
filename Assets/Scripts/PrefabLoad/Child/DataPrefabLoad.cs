using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class DataPrefabLoad : PrefabLoadBase
{
    Dictionary<EDataType, TextAsset> _dataDict = new Dictionary<EDataType, TextAsset>();

    public override async Task LoadPrefab()
    {
        if (_addressableManager == null)
            Init();

        for (int i = 0; i < (int)EDataType.Max; i++)
        {
            TextAsset prefab = await _addressableManager.GetAddressableAsset<TextAsset>($"{(EDataType)i}");
            if (prefab != null && !_dataDict.ContainsKey((EDataType)i))
                _dataDict.Add((EDataType)i, prefab);
        }
    }

    public override TextAsset GetPrefabTextAsset<TEnum>(TEnum type)
    {
        EDataType key = (EDataType)(object)type;
        return _dataDict[key];
    }
}