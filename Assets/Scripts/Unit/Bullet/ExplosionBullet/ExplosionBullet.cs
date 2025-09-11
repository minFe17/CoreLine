using UnityEngine;

public class ExplosionBullet : Bullet
{
    [SerializeField] GameObject _bullet;
    [SerializeField] GameObject _effect;

    public override void Init(Monster target, int damage)
    {
        base.Init(target, damage);
        _bullet.SetActive(true);
        _effect.SetActive(false);
    }

    void PlayEffect()
    {
        _effect.SetActive(true);
    }

    public void HitMonster(Monster monster)
    {
        monster.TakeDamage(_damage);
    }

    protected override void CheckTrigger(Collider2D collision)
    {
        if (collision.gameObject == _target.gameObject)
        {
            _bullet.SetActive(false);
            _target.TakeDamage(_damage);
            PlayEffect();
        }
    }
}