using UnityEngine;
using Utils;

public class KingUnit : Unit
{
    EUnitType _unitType = EUnitType.King;

    public int GetHPRatio() => _currentHp / _unitStateData.HP;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (_unitStateData == null)
            _unitStateData = SimpleSingleton<UnitDataList>.Instance.GetUnitData(_unitType).LevelData[0].UnitState;
        _currentHp = _unitStateData.HP;
        _isDie = false;
    }

    public override void ClickUnit()
    {
        SimpleSingleton<AttackRangeManager>.Instance.HideAttackRange();
    }

    public override void Die()
    {
        base.Die();
        // 게임오버
    }

    public override void Remove()
    {
        base.Remove();
        MonoSingleton<ObjectPoolManager>.Instance.Push(_unitType, gameObject);
    }
}