using UnityEngine;
using Utils;

public class OutlawAttack : AttackBase
{
    [SerializeField] Transform _bulletPosition;

    EAttackType _attackType;
    Vector3Int _leftPosition;
    Vector3Int _rightPosition;

    int _attackCount;
    int _maxCannonballCount = 4;

    private void OnEnable()
    {
        MapManager.Instance.GetNavFrame(out Vector3Int originCell, out _rightPosition, out Vector3 cellSize);
        _leftPosition = new Vector3Int(-_rightPosition.x, _rightPosition.y);
    }

    void Fire()
    {
        if (_unit.TargetList.Count == 0)
            return;
        GameObject temp = MonoSingleton<ObjectPoolManager>.Instance.Pull(EBulletType.Bullet);
        temp.transform.position = _bulletPosition.position;
        temp.transform.rotation = transform.rotation;
        temp.GetComponent<Bullet>().Init(_unit.TargetList[0], _unit.UnitStateData.AttackDamage);
    }

    void SpawnCannonball()
    {
        EDirectionType direction = EDirectionType.Max;
        float yRotation = transform.eulerAngles.y;

        if (Mathf.Approximately(yRotation, 0f))
            direction = EDirectionType.Right;
        else
            direction = EDirectionType.Left;

        int randomCount = Random.Range(1, _maxCannonballCount);
        for (int i = 0; i < randomCount; i++)
        {
            GameObject temp = MonoSingleton<ObjectPoolManager>.Instance.Pull(EBulletType.Cannonball);
            if (direction == EDirectionType.Right)
                temp.transform.position = _leftPosition;
            else
                temp.transform.position = _rightPosition;

            int random = Random.Range(0, MonsterManager.Instance.Monsters.Count);
            Monster monster = MonsterManager.Instance.Monsters[random].GetComponent<Monster>();
            temp.GetComponent<Cannonball>().Init(monster, _unit.UnitStateData.AttackDamage);
        }
    }

    protected override void PlayAttackAnimation()
    {
        _attackCount++;
        if (_attackCount >= 10)
        {
            _attackType = EAttackType.Skill;
            _unit.Animator.SetTrigger("doSkill");
            _attackCount = 0;
            return;
        }
        _attackType = EAttackType.Attack;
        _unit.Animator.SetTrigger("doAttack");
    }

    public override void Attack()
    {
        if (_attackType == EAttackType.Attack)
            Fire();
        else if (_attackType == EAttackType.Skill)
            SpawnCannonball();
    }
}