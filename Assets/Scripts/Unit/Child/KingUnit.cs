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
        if (_data == null)
            _data = SimpleSingleton<UnitDataList>.Instance.GetUnitData(_unitType).LevelData[_level];
        _currentHp = _data.UnitState.HP;
        _isDie = false;
    }

    public override void Die()
    {
        base.Die();
        MonoSingleton<ObjectPoolManager>.Instance.Push(_unitType, gameObject);
        //GAmeOver
    }
}