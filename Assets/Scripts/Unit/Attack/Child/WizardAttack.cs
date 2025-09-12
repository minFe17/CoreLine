using UnityEngine;
using Utils;

public class WizardAttack : AttackBase
{
    public override void Attack()
    {
        GameObject temp = MonoSingleton<ObjectPoolManager>.Instance.Pull(EBulletType.Lazer);
        if (_unit.TargetList.Count == 0)
            return;
        temp.transform.position = _unit.TargetList[0].transform.position;
        _unit.TargetList[0].TakeDamage(_unit.UnitStateData.AttackDamage);
        PlaySFX(ESFXType.WizardAttack);
    }
}