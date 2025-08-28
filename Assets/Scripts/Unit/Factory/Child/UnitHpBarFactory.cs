using UnityEngine;
using Utils;

public class UnitHpBarFactory : IFactory
{
    GameObject _hpBarPrefab;

    #region Interface
    GameObject IFactory.Create()
    {
        if (_hpBarPrefab == null)
            _hpBarPrefab = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.UI).GetPrefab(EUIPrefabType.UnitHpBar);
        return Object.Instantiate(_hpBarPrefab);
    }

    void IFactory.Register()
    {
        MonoSingleton<ObjectPoolManager>.Instance.RegisterFactory(EUIPrefabType.UnitHpBar, this);
    }
    #endregion
}