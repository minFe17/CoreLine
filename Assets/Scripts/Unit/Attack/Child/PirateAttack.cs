using UnityEngine;
using Utils;

public class PirateAttack : AttackBase
{
    [SerializeField] Transform _bulletPosition;

    public override void Attack()
    {
        if (_unit.TargetList[0] == null)
            return;
        GameObject temp = MonoSingleton<ObjectPoolManager>.Instance.Pull(EBulletType.Bullet);
        temp.transform.position = _bulletPosition.position;
        temp.transform.rotation = transform.rotation;
        temp.GetComponent<Bullet>().Init(_unit.TargetList[0], _unit.UnitStateData.AttackDamage);
    }
}