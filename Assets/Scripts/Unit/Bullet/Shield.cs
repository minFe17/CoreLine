using System.Collections.Generic;
using UnityEngine;

public class Shield : Bullet
{
    List<Monster> _monsterList = new List<Monster>();
    int _count;
    float _hitDistance = 0.5f;

    public void Init(List<Monster> monsterList, int damage)
    {
        base.Init(monsterList[0], damage);
        _monsterList = monsterList;
        _count = 0;
    }

    void NextTarget()
    {
        _count++;
        if (_count >= _monsterList.Count-1)
        {
            Remove();
            return;
        }

        while (_count < _monsterList.Count)
        {
            Monster candidate = _monsterList[_count];
            if (candidate != null && !candidate.IsDead)
            {
                _target = candidate;
                return;
            }
            _count++;
        }
        Remove();
    }

    protected override void Move()
    {
        base.Move();

        if (_target != null)
        {
            float distance = Vector3.Distance(transform.position, _target.transform.position);
            if (distance <= _hitDistance)
            {
                _target.TakeDamage(_damage);
                NextTarget();
            }
        }
    }
}