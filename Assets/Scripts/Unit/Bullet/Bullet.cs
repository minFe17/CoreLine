using UnityEngine;
using Utils;

public class Bullet : MonoBehaviour
{
    [SerializeField] EBulletType _bulletType;
    [SerializeField] protected float _speed;

    protected Monster _target;
    protected int _damage;

    public virtual void Init(Monster target, int damage)
    {
        _target = target;
        _damage = damage;
    }

    void Update()
    {
        LookTarget();
        Move();
        CheckTarget();
    }

    protected virtual void CheckTrigger(Collider2D collision)
    {
        if (collision.gameObject == _target.gameObject)
        {
            _target.TakeDamage(_damage);
            Remove();
        }
    }

    protected virtual void Move()
    {
        if (_target == null)
            return;

        Vector3 direction = (_target.transform.position - transform.position).normalized;
        transform.position += direction * _speed * Time.deltaTime;
    }

    void LookTarget()
    {
        if (_target == null)
            return;
        Vector3 targetPosition = _target.transform.position;
        Vector3 direction = targetPosition - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void CheckTarget()
    {
        if (_target.IsDead)
            MonoSingleton<ObjectPoolManager>.Instance.Push(_bulletType, gameObject);
    }

    protected void Remove()
    {
        MonoSingleton<ObjectPoolManager>.Instance.Push(_bulletType, gameObject);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        CheckTrigger(collision);
    }
}