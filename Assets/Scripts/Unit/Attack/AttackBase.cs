using UnityEngine;
using Utils;

public abstract class AttackBase : MonoBehaviour
{
    protected Unit _unit;
    protected float _attackTimer;

    public abstract void Attack();

    public Unit Unit { get => _unit; }

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

    protected virtual bool CheckAttack()
    {
        if (_unit.TargetList.Count == 0)
            return false;
        return true;
    }

    protected virtual void PlayAttackAnimation()
    {
        _unit.Animator.SetTrigger("doAttack");
    }

    void AttackTimer()
    {
        if (_unit.IsStopAttack)
            return;
        if (_unit.IsDie)
            return;
        _attackTimer += Time.deltaTime;
        if (!CheckAttack())
            return;
        if (_unit.UnitStateData.AttackSpeed <= _attackTimer)
        {
            _attackTimer = 0;
            PlayAttackAnimation();
        }
    }

    protected void PlaySFX(ESFXType type)
    {
        SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorType.PlayAudio, type);
    }

    void HandleUpgrade()
    {
        if (_unit is TowerUnit towerUnit)
        {
            AttackEvent temp = towerUnit.GetCurrentUnit().AddComponent<AttackEvent>();
            temp.Init(this);
        }
    }
}