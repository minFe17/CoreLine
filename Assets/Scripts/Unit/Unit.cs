using System.Collections.Generic;
using UnityEngine;
using Utils;

public class Unit : MonoBehaviour
{
    List<Monster> _currentMonsters = new List<Monster>();
    List<Monster> _orderedMonsters = new List<Monster>();

    protected UnitState _unitStateData;
    protected Animator _animator;
    protected Vector3Int _cell;

    protected int _currentHp;
    protected bool _isDie;

    Vector3 _leftDirection = new Vector3(0, 180f, 0f);
    HpBar _hpBar;
    int _monsterLayer;
    bool _isStopAttack;

    public IReadOnlyList<Monster> TargetList { get => _orderedMonsters; }
    public Animator Animator { get => _animator; }
    public Vector3Int Cell { get => _cell; set => _cell = value; }
    public UnitState UnitStateData { get => _unitStateData; }
    public HpBar HpBar { get => _hpBar; set => _hpBar = value; }
    public UnitUI UnitUI { get; set; }
    public bool IsDie { get => _isDie; }
    public int CurrentHp { get => _currentHp; }
    public bool IsStopAttack { get=> _isStopAttack; }

    void Start()
    {
        _monsterLayer = 1 << LayerMask.NameToLayer("Monster");
    }

    public virtual void ClickUnit()
    {
        if (_unitStateData.AttackRange != 0)
            SimpleSingleton<AttackRangeManager>.Instance.CheckAttackRange(this);
        else
            SimpleSingleton<AttackRangeManager>.Instance.HideAttackRange();
    }

    public virtual void Die()
    {
        Remove();
    }

    public virtual void Remove()
    {
        UnregisterCell();
        MonoSingleton<ObjectPoolManager>.Instance.Push(EUIPrefabType.UnitHpBar, _hpBar.gameObject);
    }

    void SetTarget()
    {
        _currentMonsters.Clear();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _unitStateData.AttackRange, _monsterLayer);

        foreach (Collider2D hit in hits)
        {
            Monster monster = hit.GetComponent<Monster>();
            if (monster != null)
            {
                _currentMonsters.Add(monster);
            }
        }

        // 나간 몬스터 처리
        for (int i = _orderedMonsters.Count - 1; i >= 0; i--)
        {
            if (!_currentMonsters.Contains(_orderedMonsters[i]))
                _orderedMonsters.RemoveAt(i);
        }

        // 새로 들어온 몬스터 처리
        foreach (Monster monster in _currentMonsters)
        {
            if (!_orderedMonsters.Contains(monster))
                _orderedMonsters.Add(monster);
        }
    }

    void LookMonster()
    {
        Monster target = _orderedMonsters[0];
        if (transform.position.x - target.transform.position.x > 0f)
            transform.rotation = Quaternion.Euler(_leftDirection);
        else
            transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    void LookMonsterSpawn()
    {
        Vector3 spawnPos = MapManager.Instance.GetSpawnWorld();
        if (transform.position.x - spawnPos.x > 0f)
            transform.rotation = Quaternion.Euler(_leftDirection);
        else
            transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    void ReturnAttack()
    {
        _isStopAttack = false;
    }

    void RestorationAttackDamage()
    {
        _unitStateData.RestorationAttackDamage();
    }

    protected void LookTarget()
    {
        SetTarget();
        if (_orderedMonsters.Count != 0)
            LookMonster();
        else
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
            _animator.SetTrigger("doHit");
    }

    public void UnregisterCell()
    {
        MapManager.Instance.UnregisterTower(_cell);
        SimpleSingleton<MapUnitManager>.Instance.RemoveUnit(_cell);
    }

    public void AddAttackDamage(int damage, float time)
    {
        _unitStateData.AddAttackDamage(damage);
        Invoke("RestorationAttackDamage", time);
    }

    public void StopAttack(float time)
    {
        _isStopAttack = true;
        Invoke("ReturnAttack", time);
    }

    public void Heal(int amount)
    {
        if (_currentHp >= _unitStateData.HP)
            return;
        _currentHp += amount;
        if(_currentHp >= _unitStateData.HP)
            _currentHp = _unitStateData.HP;
        GameObject temp = MonoSingleton<ObjectPoolManager>.Instance.Pull(EBulletType.Health_Up);
        temp.transform.position = transform.position;
        _hpBar.ChangeHp((float)_currentHp/_unitStateData.HP);
    }
}