using UnityEngine;

public class Cannonball : Bullet
{
    [SerializeField] GameObject _effect;
    [SerializeField] float _lifeTime;

    SpriteRenderer _spriteRenderer;
    TrailRenderer _trailRenderer;
    Vector3 _targetPosition;
    bool _reachedTarget = false;

    public override void Init(Monster target, int damage)
    {
        base.Init(target, damage);
        if(_trailRenderer == null)
            _trailRenderer = GetComponent<TrailRenderer>();
        if(_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
        _trailRenderer.Clear();
        SetTarget();
        _reachedTarget = false;
        _effect.SetActive(false);
        _spriteRenderer.color = Color.white;
    }

    void Update()
    {
        if (!_reachedTarget)
            Move();
    }

    void SetTarget()
    {
        _targetPosition = _target.transform.position;

        Vector3 direction = _targetPosition - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void PlayEffect()
    {
        _effect.SetActive(true);
        _spriteRenderer.color = new Color(1,1,1,0);
        _trailRenderer.Clear();
    }

    public void HitMonster(Monster monster)
    {
        monster.TakeDamage(_damage);
    }

    protected override void Move()
    {
        Vector3 direction = (_targetPosition - transform.position).normalized;
        float distanceThisFrame = _speed * Time.deltaTime;
        float distanceToTarget = Vector3.Distance(transform.position, _targetPosition);

        if (distanceThisFrame >= distanceToTarget)
        {
            transform.position = _targetPosition;
            _reachedTarget = true;

            PlayEffect();
        }
        else
            transform.position += direction * distanceThisFrame;

        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
    }
}
