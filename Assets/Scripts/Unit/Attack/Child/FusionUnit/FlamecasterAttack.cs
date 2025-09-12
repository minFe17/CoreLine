using UnityEngine;
using Utils;

public class FlamecasterAttack : AttackBase
{
    [SerializeField] Transform _meteorPosition;

    public override void Attack()
    {
        if (_unit.TargetList.Count == 0)
            return;
        GameObject temp = MonoSingleton<ObjectPoolManager>.Instance.Pull(EBulletType.Meteor);
        temp.transform.position = _meteorPosition.position;
        temp.transform.rotation = transform.rotation;
        temp.GetComponent<Bullet>().Init(_unit.TargetList[0], _unit.UnitStateData.AttackDamage);
        PlaySFX(ESFXType.FireAttack);
    }
}