using UnityEngine;
using Utils;

public class FusionUnit : Unit
{
    [SerializeField] EFusionUnitType _unitType;

    void OnEnable()
    {
        _unitStateData = SimpleSingleton<FusionDataList>.Instance.GetFusionData(_unitType).FusionUnitState;
        _currentHp = _unitStateData.HP;
    }

    public override void Die()
    {
        base.Die();
        MonoSingleton<ObjectPoolManager>.Instance.Push(_unitType, gameObject);
    }
}
