using System.Threading.Tasks;
using UnityEngine;

public class MapPrefabLoad : PrefabLoadBase
{
    GameObject _mapPrefab;
    string _name;

    public override void Init()
    {
        base.Init();
    }
    public void GetPrefabName(string id)
    {
        _name = id;
    }
    public override async Task LoadPrefab()
    {
        if (_addressableManager == null)
            Init();
        _mapPrefab = await _addressableManager.GetAddressableAsset<GameObject>(_name);
    }

    public override GameObject GetPrefab()
    {
        return _mapPrefab;
    }
}