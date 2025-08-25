using UnityEngine;
using Utils;
public class FusionUnitFactory : IFactory
{
    GameObject _unitPrefab;
    EFusionUnitType _unitType;

    public FusionUnitFactory(EFusionUnitType unitType)
    {
        _unitType = unitType;
    }

    GameObject IFactory.Create()
    {
        if (_unitPrefab == null)
            _unitPrefab = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.FusionUnit).GetPrefab(_unitType);
        return Object.Instantiate(_unitPrefab);
    }

    void IFactory.Register()
    {
        MonoSingleton<ObjectPoolManager>.Instance.RegisterFactory(_unitType, this);
    }
}