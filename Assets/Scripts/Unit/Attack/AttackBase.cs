using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    TowerUnit _unit;
    float _attackTimer;

    public abstract void Attack();

    void Start()
    {
        _unit = GetComponent<TowerUnit>();
        _unit.OnUpgrade += HandleUpgrade;
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
        if(_unit.Data.UnitState.AttackSpeed <= _attackTimer)
        {
            _attackTimer = 0;
            PlayAttackAnimation();
        }
    }

    void HandleUpgrade()
    {
        AttackEvent temp = _unit.GetCurrentUnit().AddComponent<AttackEvent>();
        temp.Init(this);
    }

    protected virtual void PlayAttackAnimation()
    {
        _unit.Animator.SetTrigger("doAttack");
    }
}