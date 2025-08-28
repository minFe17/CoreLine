using UnityEngine;
using Utils;

public class Unit : MonoBehaviour
{
    Vector3Int _cell;
    HpBar _hpBar;

    protected UnitState _unitStateData;
    protected Animator _animator;
    protected int _level;

    protected int _currentHp;
    protected bool _isDie;

    public Animator Animator { get => _animator; }
    public bool IsDie { get => _isDie; }
    public Vector3Int Cell { get => _cell; set => _cell = value; }
    public UnitState UnitStateData { get => _unitStateData; }
    public HpBar HpBar { set => _hpBar = value; }

    public virtual void ClickUnit()
    {
        if (_unitStateData.AttackRange != 0)
            SimpleSingleton<AttackRangeManager>.Instance.CheckAttackRange(this);
        else
            SimpleSingleton<AttackRangeManager>.Instance.HideAttackRange();
    }

    public void TakeDamage(int damage)
    {
        if (_isDie)
            return;

        _currentHp -= damage;
        _hpBar.ChangeHp((float)_currentHp / _unitStateData.HP);

        if (_currentHp <= 0)
        {
            _animator.SetTrigger("doDie");
        }
        else
        {
            _animator.SetTrigger("doHit");
        }
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
        MonoSingleton<ObjectPoolManager>.Instance.Push(EUIPrefabType.UnitHpBar, _hpBar.gameObject);
    }
}