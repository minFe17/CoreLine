using UnityEngine;
using Utils;

public class UnitFactory : IFactory
{
    GameObject _unitPrefab;
    EUnitType _unitType;

    public UnitFactory(EUnitType unitType)
    {
        _unitType = unitType;
    }

    GameObject IFactory.Create()
    {
        if (_unitPrefab == null)
            _unitPrefab = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.Unit).GetPrefab(_unitType);
        return Object.Instantiate(_unitPrefab);
    }

    void IFactory.Register()
    {
        MonoSingleton<ObjectPoolManager>.Instance.RegisterFactory(_unitType, this);
    }
}