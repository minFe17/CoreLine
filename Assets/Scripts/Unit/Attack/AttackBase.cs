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
        if(_unit.Data.AttackSpeed <= _attackTimer)
        {
            _attackTimer = 0;
            _unit.Animator.SetTrigger("doAttack");
        }
    }

    void HandleUpgrade()
    {
        AttackEvent temp = _unit.GetCurrentUnit().AddComponent<AttackEvent>();
        temp.Init(this);
    }
}