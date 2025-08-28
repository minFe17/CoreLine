using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    Unit _unit;
    Vector3 _leftDirection = new Vector3(0, 180f, 0f);

    float _attackTimer;

    public abstract void Attack();

    void Start()
    {
        _unit = GetComponent<Unit>();
        if(_unit is TowerUnit towerUnit)
            towerUnit.OnUpgrade += HandleUpgrade;
        HandleUpgrade();
    }

    void Update()
    {
        AttackTimer();
        LookTarget();
    }

    void AttackTimer()
    {
        if(_unit.IsDie)
            return;
        _attackTimer += Time.deltaTime;
        if(_unit.UnitStateData.AttackSpeed <= _attackTimer)
        {
            _attackTimer = 0;
            PlayAttackAnimation();
        }
    }

    void HandleUpgrade()
    {
        if (_unit is TowerUnit towerUnit)
        {
            AttackEvent temp = towerUnit.GetCurrentUnit().AddComponent<AttackEvent>();
            temp.Init(this);
        }
    }

    void LookTarget()
    {
        // 타겟이 없으면
        LookMonsterSpawn();
    }

    void LookMonsterSpawn()
    {
        Vector3 spawnPos = MapManager.Instance.GetSpawnWorld();
        if (transform.position.x - spawnPos.x > 0f)
            transform.rotation = Quaternion.Euler(_leftDirection);
        else
            transform.rotation = Quaternion.Euler(Vector3.zero);
    }


    protected virtual void PlayAttackAnimation()
    {
        _unit.Animator.SetTrigger("doAttack");
    }
}