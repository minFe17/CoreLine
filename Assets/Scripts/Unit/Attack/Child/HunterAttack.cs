using UnityEngine;
using Utils;

public class HunterAttack : AttackBase
{
    [SerializeField] Transform _arrowPosition;

    public override void Attack()
    {
        int count = _unit.TargetList.Count > 3 ? 3 : _unit.TargetList.Count;
        for(int i=0; i< count; i++)
        {
            if (_unit.TargetList[i] == null)
                continue;
            GameObject temp = MonoSingleton<ObjectPoolManager>.Instance.Pull(EBulletType.Arrow);
            temp.transform.position = _arrowPosition.position;
            temp.transform.rotation = transform.rotation;
            temp.GetComponent<Bullet>().Init(_unit.TargetList[i], _unit.UnitStateData.AttackDamage);
        }
    }
}
