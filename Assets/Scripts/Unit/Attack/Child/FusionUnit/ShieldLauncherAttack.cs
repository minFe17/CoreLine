using System.Collections.Generic;
using UnityEngine;
using Utils;

public class ShieldLauncherAttack : AttackBase
{
    [SerializeField] Transform _shieldPosition;

    int _minShieldAttack = 2;
    int _maxShieldAttack = 7;

    public override void Attack()
    {
        if (_unit.TargetList.Count == 0)
            return;
        GameObject temp = MonoSingleton<ObjectPoolManager>.Instance.Pull(EBulletType.Shield);
        temp.transform.position = _shieldPosition.position;
        temp.transform.rotation = transform.rotation;
        temp.GetComponent<Shield>().Init(SetTargetMonster(), _unit.UnitStateData.AttackDamage);
        PlaySFX(ESFXType.ShieldAttack);
    }

    List<Monster> SetTargetMonster()
    {
        List<Monster> monsterList = new List<Monster>();
        Vector2 currentPos = transform.position;
        float maxDistance = 10f;

        Monster lastMonster = null;
        lastMonster = _unit.TargetList[0];
        monsterList.Add(lastMonster);

        int randomCount = Random.Range(_minShieldAttack, _maxShieldAttack);

        for (int i = 0; i < randomCount; i++)
        {
            Monster closestMonster = null;
            float minDist = 10f;

            foreach (MonsterMover monsterMover in MonsterManager.Instance.Monsters)
            {
                Monster monster = monsterMover.GetComponent<Monster>();
                if (monster == null || monster.IsDead || monster == lastMonster)
                    continue;

                float dist = Vector2.Distance(currentPos, monster.transform.position);
                if (dist <= maxDistance && dist < minDist)
                {
                    minDist = dist;
                    closestMonster = monster;
                }
            }

            if (closestMonster != null && closestMonster != lastMonster)
            {
                lastMonster = closestMonster;
                monsterList.Add(closestMonster);
            }
            else
                break;
        }
        return monsterList;
    }
}