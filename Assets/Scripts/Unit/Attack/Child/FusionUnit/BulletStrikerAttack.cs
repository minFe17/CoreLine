using UnityEngine;
using Utils;

public class BulletStrikerAttack : AttackBase
{
    [SerializeField] Transform _bulletPosition;

    EAttackType _attackType;
    int _bulletCount;
    int _maxBullet = 20;
    int _explosionBulletCount;
    int _maxaExplosionBullet = 10;

    private void OnEnable()
    {
        _attackType = EAttackType.Attack;
    }

    void FireBullet()
    {
        _bulletCount++;
        CreateBullet(EBulletType.Bullet);
        PlaySFX(ESFXType.Gun);


        if (_bulletCount >= _maxBullet)
        {
            _bulletCount = 0;
            _attackType = EAttackType.Skill;
        }
    }

    void FireExplosionBullet()
    {
        _explosionBulletCount++;
        CreateBullet(EBulletType.ExplosionBullet);
        PlaySFX(ESFXType.Attack);

        if (_explosionBulletCount >= _maxaExplosionBullet)
        {
            _explosionBulletCount = 0;
            _attackType = EAttackType.Attack;
        }
    }

    void CreateBullet(EBulletType bulletType)
    {
        GameObject temp = MonoSingleton<ObjectPoolManager>.Instance.Pull(bulletType);
        temp.transform.position = _bulletPosition.position;
        temp.transform.rotation = transform.rotation;
        temp.GetComponent<Bullet>().Init(_unit.TargetList[0], _unit.UnitStateData.AttackDamage);
    }

    public override void Attack()
    {
        if (_unit.TargetList.Count == 0)
            return;

        if(_attackType == EAttackType.Attack)
            FireBullet();
        else
            FireExplosionBullet();
    }
}