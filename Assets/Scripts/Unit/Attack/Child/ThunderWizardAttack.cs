using UnityEngine;
using Utils;

public class ThunderWizardAttack : AttackBase
{
    [SerializeField] Transform _thunderSpearPosition;

    public override void Attack()
    {
        if (_unit.TargetList.Count == 0)
            return;
        GameObject temp = MonoSingleton<ObjectPoolManager>.Instance.Pull(EBulletType.ThunderSpear);
        temp.transform.position = _thunderSpearPosition.position;
        temp.transform.rotation = transform.rotation;
        temp.GetComponent<Bullet>().Init(_unit.TargetList[0], _unit.UnitStateData.AttackDamage);
        PlaySFX(ESFXType.ThunderAttack);
    }
}