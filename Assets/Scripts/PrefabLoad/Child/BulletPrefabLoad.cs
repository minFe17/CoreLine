using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
public class BulletPrefabLoad : PrefabLoadBase
{
    Dictionary<EBulletType, GameObject> _bulletDict = new Dictionary<EBulletType, GameObject>();

    public override async Task LoadPrefab()
    {
        if (_addressableManager == null)
            Init();

        for (int i = 0; i < (int)EBulletType.Max; i++)
        {
            GameObject prefab = await _addressableManager.GetAddressableAsset<GameObject>($"{(EBulletType)i}");
            if (prefab != null && !_bulletDict.ContainsKey((EBulletType)i))
                _bulletDict.Add((EBulletType)i, prefab);
        }
    }

    public override GameObject GetPrefab<TEnum>(TEnum type)
    {
        EBulletType key = (EBulletType)(object)type;
        return _bulletDict[key];
    }
}