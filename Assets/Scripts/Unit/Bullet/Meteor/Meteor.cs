using UnityEngine;

public class Meteor : Bullet
{
    [SerializeField] GameObject _meteor;
    [SerializeField] GameObject _effect;

    int _minSmallMeteor;
    int _maxSmallMeteor;

    public override void Init(Monster target, int damage)
    {
        base.Init(target, damage);
        _meteor.SetActive(true);
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

    public void SpawnSmallMeteor()
    {
        // 원형으로
    }

    protected override void CheckTrigger(Collider2D collision)
    {
        if (collision.gameObject == _target.gameObject)
        {
            _meteor.SetActive(false);
            _target.TakeDamage(_damage);
            PlayEffect();
        }
    }
}