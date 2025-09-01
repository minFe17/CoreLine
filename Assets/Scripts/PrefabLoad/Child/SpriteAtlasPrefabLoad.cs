using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.U2D;

public class SpriteAtlasPrefabLoad : PrefabLoadBase
{
    Dictionary<EAtlasPrefabType, SpriteAtlas> _atlasDict = new Dictionary<EAtlasPrefabType, SpriteAtlas>();

    public override async Task LoadPrefab()
    {
        if (_addressableManager == null)
            Init();

        for (int i = 0; i < (int)EAtlasPrefabType.Max; i++)
        {
            SpriteAtlas prefab = await _addressableManager.GetAddressableAsset<SpriteAtlas>($"{(EAtlasPrefabType)i}");
            if (prefab != null && !_atlasDict.ContainsKey((EAtlasPrefabType)i))
                _atlasDict.Add((EAtlasPrefabType)i, prefab);
        }
    }

    public override SpriteAtlas GetPrefabAtlas<TEnum>(TEnum type)
    {
        EAtlasPrefabType key = (EAtlasPrefabType)(object)type;
        return _atlasDict[key];
    }
}