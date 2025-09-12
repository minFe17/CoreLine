using UnityEngine;

public class HammerAttack : AttackBase
{
    [SerializeField] ParticleSystem _attackEffect;

    public override void Attack()
    {
        if (_unit.TargetList.Count == 0)
            return;
        for(int i=0; i< _unit.TargetList.Count; i++)
            _unit.TargetList[i].TakeDamage(_unit.UnitStateData.AttackDamage);
        _attackEffect.Play();
        PlaySFX(ESFXType.Attack);
    }
}