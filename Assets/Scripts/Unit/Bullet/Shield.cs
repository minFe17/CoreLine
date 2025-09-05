using System.Collections.Generic;
using UnityEngine;

public class Shield : Bullet
{
    List<Monster> _monsterList = new List<Monster>();
    int _count;

    public void Init(List<Monster> monsterList, int damage)
    {
        base.Init(monsterList[0], damage);
        _monsterList = monsterList;
        _count = 0;
    }

    void NextTarget()
    {
        Debug.Log($"count : {_count}");

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

    // 트리거 체크 X 
    // 트리거 체크는 데미지만 주던가 하지말고
    // 거리 가까운지 체크?
    protected override void CheckTrigger(Collider2D collision)
    {
        if (collision.gameObject == _target.gameObject)
        {
            _target.TakeDamage(_damage);
            NextTarget();
        }
    }
}