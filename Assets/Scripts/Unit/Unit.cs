using UnityEngine;
using Utils;

public class Unit : MonoBehaviour
{
    Vector3Int _cell;
    protected UnitLevelData _data;
    protected Animator _animator;
    protected int _level;

    protected int _currentHp;
    protected bool _isDie;

    public UnitLevelData Data { get => _data; }
    public Animator Animator { get => _animator; }
    public bool IsDie { get => _isDie; }
    public Vector3Int Cell { get => _cell; set => _cell = value; }

    protected virtual void CheckAttackRange()
    {
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

    public void UnregisterCell()
    {
        MapManager.Instance.UnregisterTower(_cell);
        SimpleSingleton<MapUnitManager>.Instance.RemoveUnit(_cell);
    }

    public virtual void Die()
    {
        _isDie = true;
        UnregisterCell();
    }
}