using UnityEngine;
using Utils;

public class Meteor : Bullet
{
    [SerializeField] GameObject _meteor;
    [SerializeField] ParticleSystem _effect;
    [SerializeField] float _lifeTime;

    int _minSmallMeteor = 3; 
    int _maxSmallMeteor = 5;
    float _smallMeteorSpawnRadius = 3f;

    Vector3 _targetDirection;
    bool _hasHit = false;

    public override void Init(Monster target, int damage)
    {
        base.Init(target, damage);
        _meteor.SetActive(true);
        _effect.gameObject.SetActive(false);
        _hasHit = false;

        SetDirection();
        Invoke(nameof(Remove), _lifeTime);
    }

    void Update()
    {
        Move();
    }

    void SetDirection()
    {
        Vector3 targetPosition = _target != null ? _target.transform.position : transform.position + Vector3.down;
        _targetDirection = (targetPosition - transform.position).normalized;

        float angle = Mathf.Atan2(_targetDirection.y, _targetDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    protected override void Move()
    {
        if (_hasHit) return;
        transform.position += _targetDirection * _speed * Time.deltaTime;

        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
    }

    protected override void CheckTrigger(Collider2D collision)
    {
        if (_hasHit) return;

        if (collision.TryGetComponent(out Monster monster))
        {
            _hasHit = true;

            _meteor.SetActive(false);
            monster.TakeDamage(_damage);
            PlayEffect();
        }
    }

    void PlayEffect()
    {
        _effect.gameObject.SetActive(true);
        _effect.GetComponent<MeteorEffect>().PlayEffect();
    }

    public void HitMonster(Monster monster)
    {
        monster.TakeDamage(_damage);
    }

    public void SpawnSmallMeteor()
    {
        int count = Random.Range(_minSmallMeteor, _maxSmallMeteor+1);

        float angleStep = 360f / count;
        Vector3 origin = transform.position;

        for (int i = 0; i < count; i++)
        {
            float angle = -i * angleStep * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f);
            Vector3 spawnPos = origin + direction * _smallMeteorSpawnRadius;

            Bullet smallMeteor = MonoSingleton<ObjectPoolManager>.Instance.Pull(EBulletType.SmallMeteor).GetComponent<Bullet>();
            smallMeteor.transform.position = spawnPos;
            int randomMonster = Random.Range(0, MonsterManager.Instance.Monsters.Count);
            smallMeteor.Init(MonsterManager.Instance.Monsters[randomMonster].GetComponent<Monster>(), _damage/3);
        }
    }
}