using UnityEngine;
using Utils;

public class KingUnit : Unit
{
    EUnitType _unitType = EUnitType.King;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (_unitStateData == null)
            _unitStateData = SimpleSingleton<UnitDataList>.Instance.GetUnitData(_unitType).LevelData[_level].UnitState;
        _currentHp = _unitStateData.HP;
        _isDie = false;
    }

    public override void Die()
    {
        base.Die();
        MonoSingleton<ObjectPoolManager>.Instance.Push(_unitType, gameObject);
        //GAmeOver
    }
}