using UnityEngine;

public class AssassinAttack : AttackBase
{
    [SerializeField] ParticleSystem _attackEffect;

    public override void Attack()
    {
        if (_unit.TargetList.Count == 0)
            return;
        _unit.TargetList[0].TakeDamage(_unit.UnitStateData.AttackDamage);
        _attackEffect.Play();
        PlaySFX(ESFXType.SwordAttack);
    }
}