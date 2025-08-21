using UnityEngine;
using Utils;

public class AttackRangeManager
{
    // ╫л╠шео
    GameObject _attackRange;
    Unit _targetUnit;

    float _attctRangeDiameter = 2f;

    bool IsSameUnit(Unit unit) => _targetUnit == unit;

    void ShowAttackRange(Unit unit)
    {
        if (_attackRange == null)
            _attackRange = MonoSingleton<ObjectPoolManager>.Instance.Pull(EPrefabType.AttackRange);

        _attackRange.transform.position = unit.transform.position;
        float size = unit.Data.AttackRange * _attctRangeDiameter;
        _attackRange.transform.localScale = new Vector2(size, size);

        _targetUnit = unit;
    }

    public void CheckAttackRange(Unit unit)
    {
        if (IsSameUnit(unit))
        {
            HideAttackRange();
            return;
        }
        ShowAttackRange(unit);
    }

    public void HideAttackRange()
    {
        if (_attackRange != null)
            MonoSingleton<ObjectPoolManager>.Instance.Push(EPrefabType.AttackRange, _attackRange);

        _attackRange = null;
        _targetUnit = null;
    }
}