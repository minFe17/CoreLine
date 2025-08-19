using UnityEngine;
using Utils;

public class AttackRangeFactory : IFactory
{
    GameObject _attackRangePrefab;

    #region Interface
    GameObject IFactory.Create()
    {
        if(_attackRangePrefab == null)
            _attackRangePrefab = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.AttackRange).GetPrefab();
        return UnityEngine.Object.Instantiate(_attackRangePrefab);
    }

    void IFactory.Register()
    {
        MonoSingleton<ObjectPoolManager>.Instance.RegisterFactory(EPrefabType.AttackRange, this);
    }
    #endregion
}