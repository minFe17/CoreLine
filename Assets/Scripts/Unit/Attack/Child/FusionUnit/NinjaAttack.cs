using UnityEngine;
using Utils;

public class NinjaAttack : AttackBase
{
    [SerializeField] Transform _daggerPosition;

    public override void Attack()
    {
        int count = _unit.TargetList.Count > 3 ? 3 : _unit.TargetList.Count;
        for (int i = 0; i < count; i++)
        {
            if (_unit.TargetList[i] == null)
                continue;
            GameObject temp = MonoSingleton<ObjectPoolManager>.Instance.Pull(EBulletType.Dagger);
            temp.transform.position = _daggerPosition.position;
            temp.transform.rotation = transform.rotation;
            temp.GetComponent<Bullet>().Init(_unit.TargetList[i], _unit.UnitStateData.AttackDamage);
        }
    }
}