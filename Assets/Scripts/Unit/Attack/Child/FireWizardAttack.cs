using UnityEngine;
using Utils;

public class FireWizardAttack : AttackBase
{
    [SerializeField] Transform _fireBallPosition;

    public override void Attack()
    {
        if (_unit.TargetList.Count == 0)
            return;
        GameObject temp = MonoSingleton<ObjectPoolManager>.Instance.Pull(EBulletType.FireBall);
        temp.transform.position = _fireBallPosition.position;
        temp.transform.rotation = transform.rotation;
        temp.GetComponent<Bullet>().Init(_unit.TargetList[0], _unit.UnitStateData.AttackDamage);
    }
}