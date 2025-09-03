using UnityEngine;

public class ThunderSpear : Bullet
{
    [SerializeField] float _lifeTime;

    Vector3 _targetDirection;

    public override void Init(Monster target, int damage)
    {
        base.Init(target, damage);
        Invoke("Remove", _lifeTime);
        SetTarget();
    }

    void Update()
    {
        Move();
    }

    void SetTarget()
    {
        Vector3 targetPosition = _target.transform.position;
        _targetDirection = targetPosition - transform.position;

        float angle = Mathf.Atan2(_targetDirection.y, _targetDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        _targetDirection = _targetDirection.normalized;
    }

    protected override void CheckTrigger(Collider2D collision)
    {
        if (collision.TryGetComponent<Monster>(out Monster monster))
            monster.TakeDamage(_damage);
    }

    protected override void Move()
    {
        transform.position += _targetDirection * _speed * Time.deltaTime;
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
    }
}