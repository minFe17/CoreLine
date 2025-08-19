using NaughtyAttributes;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] protected EUnitType _unitType;
    [ShowIf("IsNotKing")]

    protected const int MaxLevel = 3;

    protected UnitLevelData _data;
    protected Animator _animator;
    protected int _level;

    protected int _currentHp;
    protected bool _isDie;

    public UnitLevelData Data { get => _data; }
    public Animator Animator { get => _animator; }
    public bool IsDie { get => _isDie; }

    #region NaughtyAttributes
    bool IsNotKing() => _unitType != EUnitType.King;
    #endregion

    public void TakeDamage(int damage)
    {
        if (_isDie)
            return;

        _currentHp -= damage;
        if (_currentHp <= 0)
            _animator.SetTrigger("doDie");
        else
            _animator.SetTrigger("doHit");
    }

    #region Animation Event
    protected virtual void Die()
    {
        _isDie = true;
        // 오브젝트 풀링

    }
    #endregion
}