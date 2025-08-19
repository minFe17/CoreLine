using System.Threading.Tasks;
using UnityEngine;

public class AttackRangePrefabLoad : PrefabLoadBase
{
    GameObject _attackRangePrefab;
    string _name;

    public override void Init()
    {
        base.Init();
        _name = "AttackRange";
    }

    public override async Task LoadPrefab()
    {
        if (_addressableManager == null)
            Init();
        _attackRangePrefab = await _addressableManager.GetAddressableAsset<GameObject>(_name);
    }

    public override GameObject GetPrefab()
    {
        return _attackRangePrefab;
    }
}