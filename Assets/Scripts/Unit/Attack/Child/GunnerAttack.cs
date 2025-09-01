using UnityEngine;
using Utils;

public class GunnerAttack : AttackBase
{
    [SerializeField] Transform _bulletPosition;
    [SerializeField] Transform _maxLevelBulletPosition;
    [SerializeField] ParticleSystem _maxLevelEffect;

    public override void Attack()
    {
        if (_unit.TargetList[0] == null)
            return;

        if(_unit is TowerUnit unit)
        {
            GameObject temp = MonoSingleton<ObjectPoolManager>.Instance.Pull(EBulletType.Bullet);

            if (unit.IsMaxLevel())
            {
                temp.transform.position = _maxLevelBulletPosition.position;
                _maxLevelEffect.Play();
            }
            else
                temp.transform.position = _bulletPosition.position;

            temp.transform.rotation = transform.rotation;
            temp.GetComponent<Bullet>().Init(_unit.TargetList[0], _unit.UnitStateData.AttackDamage);
        }
    }
}