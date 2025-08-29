using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    float _attackTimer;

    protected Unit _unit;

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

    protected virtual void PlayAttackAnimation()
    {
        _unit.Animator.SetTrigger("doAttack");
    }
}