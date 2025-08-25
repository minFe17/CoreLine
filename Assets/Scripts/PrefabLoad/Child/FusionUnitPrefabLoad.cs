using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class FusionUnitPrefabLoad : PrefabLoadBase
{
    Dictionary<EFusionUnitType, GameObject> _unitDict = new Dictionary<EFusionUnitType, GameObject>();
    public override async Task LoadPrefab()
    {
        if (_addressableManager == null)
            Init();

        for (int i = 0; i < (int)EFusionUnitType.Max; i++)
        {
            GameObject prefab = await _addressableManager.GetAddressableAsset<GameObject>($"{(EFusionUnitType)i}");
            if (prefab != null && !_unitDict.ContainsKey((EFusionUnitType)i))
                _unitDict.Add((EFusionUnitType)i, prefab);
        }
    }

    public override GameObject GetPrefab<TEnum>(TEnum type)
    {
        EFusionUnitType key = (EFusionUnitType)(object)type;
        return _unitDict[key];
    }
}