using UnityEngine;
using Utils;

public class Unit : MonoBehaviour
{
    Vector3 _leftDirection = new Vector3(0, 180f, 0f);
    HpBar _hpBar;

    protected UnitState _unitStateData;
    protected Animator _animator;
    protected Vector3Int _cell;

    protected int _level;
    protected int _currentHp;
    protected bool _isDie;


    public Animator Animator { get => _animator; }
    public bool IsDie { get => _isDie; }
    public Vector3Int Cell { get => _cell; set => _cell = value; }
    public UnitState UnitStateData { get => _unitStateData; }
    public HpBar HpBar { get => _hpBar; set => _hpBar = value; }

    public virtual void ClickUnit()
    {
        if (_unitStateData.AttackRange != 0)
            SimpleSingleton<AttackRangeManager>.Instance.CheckAttackRange(this);
        else
            SimpleSingleton<AttackRangeManager>.Instance.HideAttackRange();
    }

    void LookMonsterSpawn()
    {
        Vector3 spawnPos = MapManager.Instance.GetSpawnWorld();
        if (transform.position.x - spawnPos.x > 0f)
            transform.rotation = Quaternion.Euler(_leftDirection);
        else
            transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    protected void LookTarget()
    {
        // 타겟이 없으면
        LookMonsterSpawn();
    }

    public void TakeDamage(int damage)
    {
        if (_isDie)
            return;

        _currentHp -= damage;
        _hpBar.ChangeHp((float)_currentHp / _unitStateData.HP);

        if (_currentHp <= 0)
        {
            _isDie = true;
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
        UnregisterCell();
        MonoSingleton<ObjectPoolManager>.Instance.Push(EUIPrefabType.UnitHpBar, _hpBar.gameObject);
    }
}