using UnityEngine;
using Utils;

public class Unit : MonoBehaviour
{
    protected UnitLevelData _data;
    protected Animator _animator;
    protected int _level;

    protected int _currentHp;
    protected bool _isDie;

    public UnitLevelData Data { get => _data; }
    public Animator Animator { get => _animator; }
    public bool IsDie { get => _isDie; }

    void OnMouseDown()
    {
        // UI 소환 

        if (_data.UnitState.AttackRange != 0)
            SimpleSingleton<AttackRangeManager>.Instance.CheckAttackRange(this);
        else
            SimpleSingleton<AttackRangeManager>.Instance.HideAttackRange();
    }

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