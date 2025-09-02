using UnityEngine;

public class WarriorAttack : AttackBase
{
    [SerializeField] ParticleSystem _attackEffect;

    public override void Attack()
    {
        if (_unit.TargetList[0] == null)
            return;
        _unit.TargetList[0].TakeDamage(_unit.UnitStateData.AttackDamage);
        _attackEffect.Play();
    }
}