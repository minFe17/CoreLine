using UnityEngine;
using Utils;

public class ArcherAttack : AttackBase
{
    [SerializeField] Transform _arrowPosition;

    public override void Attack()
    {
        if (_unit.TargetList.Count == 0)
            return;
        GameObject temp = MonoSingleton<ObjectPoolManager>.Instance.Pull(EBulletType.Arrow);
        temp.transform.position = _arrowPosition.position;
        temp.transform.rotation = transform.rotation;
        temp.GetComponent<Bullet>().Init(_unit.TargetList[0], _unit.UnitStateData.AttackDamage);

        PlaySFX(ESFXType.ArrowAttack);
    }
}