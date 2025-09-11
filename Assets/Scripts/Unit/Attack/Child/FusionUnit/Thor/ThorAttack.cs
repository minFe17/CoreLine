using System.Collections.Generic;
using UnityEngine;
using Utils;

public class ThorAttack : AttackBase
{
    int _minThunderCount = 3;
    int _maxThunderCount = 7;

    public override void Attack()
    {
        if (_unit.TargetList.Count == 0)
            return;
        List<Monster> targetList = SetTargetMonster();
        for (int i = 0; i < targetList.Count; i++)
        {
            ChainThunder chainThunder = MonoSingleton<ObjectPoolManager>.Instance.Pull(EBulletType.ChainThunder).GetComponent<ChainThunder>();
            if (i == 0)
                chainThunder.Init(transform.position, targetList[i].transform.position);
            else
                chainThunder.Init(targetList[i-1].transform.position, targetList[i].transform.position);

            targetList[i].TakeDamage(_unit.UnitStateData.AttackDamage);
        }
    }

    List<Monster> SetTargetMonster()
    {
        List<Monster> monsterList = new List<Monster>();
        HashSet<Monster> addedMonsters = new HashSet<Monster>();

        Vector2 currentPos = transform.position;
        float maxDistance = 7f;

        Monster lastMonster = _unit.TargetList[0];
        if (lastMonster == null || lastMonster.IsDead)
            return monsterList;

        monsterList.Add(lastMonster);
        addedMonsters.Add(lastMonster);

        int randomCount = Random.Range(_minThunderCount, _maxThunderCount);

        for (int i = 0; i < randomCount; i++)
        {
            Monster closestMonster = null;
            float minDist = maxDistance;

            foreach (MonsterMover monsterMover in MonsterManager.Instance.Monsters)
            {
                Monster monster = monsterMover.GetComponent<Monster>();
                if (monster == null || monster.IsDead || addedMonsters.Contains(monster))
                    continue;

                float dist = Vector2.Distance(currentPos, monster.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestMonster = monster;
                }
            }

            if (closestMonster != null)
            {
                lastMonster = closestMonster;
                monsterList.Add(closestMonster);
                addedMonsters.Add(closestMonster);
            }
            else
                break;
        }
        return monsterList;
    }
}