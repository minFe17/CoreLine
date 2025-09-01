using UnityEngine;
using Utils;

public class BulletFactory : IFactory
{
    GameObject _bulletPrefab;
    EBulletType _bulletType;

    public BulletFactory(EBulletType bulletType)
    {
        _bulletType = bulletType;
    }

    GameObject IFactory.Create()
    {
        if (_bulletPrefab == null)
            _bulletPrefab = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.Bullet).GetPrefab(_bulletType);
        return Object.Instantiate(_bulletPrefab);
    }

    void IFactory.Register()
    {
        MonoSingleton<ObjectPoolManager>.Instance.RegisterFactory(_bulletType, this);
    }
}