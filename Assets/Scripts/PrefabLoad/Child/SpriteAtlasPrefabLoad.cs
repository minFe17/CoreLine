using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.U2D;
using Utils;

public class SpriteAtlasPrefabLoad : PrefabLoadBase
{
    SpriteAtlas _atlasPrefab;
    string _name;

    public override void Init()
    {
        base.Init();
        _name = "UnitSpriteAtlas";
    }

    public override async Task LoadPrefab()
    {
        if (_addressableManager == null)
            Init();
        _atlasPrefab = await _addressableManager.GetAddressableAsset<SpriteAtlas>(_name);
    }

    public override T GetPrefab<T>()
    {
        if (typeof(T) == typeof(SpriteAtlas))
            return (T)(object)_atlasPrefab;
        return default(T);
    }
}
