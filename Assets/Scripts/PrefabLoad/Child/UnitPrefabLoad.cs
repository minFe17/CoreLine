using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class UnitPrefabLoad : PrefabLoadBase
{
    Dictionary<EUnitType, GameObject> _unitDict = new Dictionary<EUnitType, GameObject>();

    public override async Task LoadPrefab()
    {
        if (_addressableManager == null)
            Init();

        // юс╫ц
        for(int i=0; i<(int)EUnitType.Max;  i++)
        {
            GameObject prefab = await _addressableManager.GetAddressableAsset<GameObject>($"{(EUnitType)i}");
            if (prefab != null && !_unitDict.ContainsKey((EUnitType)i))
                _unitDict.Add((EUnitType)i, prefab);
        }
    }

    public override GameObject GetPrefab<TEnum>(TEnum type)
    {
        EUnitType key = (EUnitType)(object)type;
        return _unitDict[key];
    }
}
