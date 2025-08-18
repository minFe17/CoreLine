using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    Unit _unit;
    Animator _animator;
    float _attackTimer;

    protected abstract void Attack();

    void Start()
    {
        _unit = GetComponent<Unit>();
        _animator = GetComponent<Animator>();
        //_attackRange.transform.localScale = new Vector2(_unit.Data.AttackRange * 2, _unit.Data.AttackRange * 2);
    }

    void Update()
    {
        AttackTimer();
    }

    void OnMouseDown()
    {
        //_attackRange.SetActive(true);
    }

    void AttackTimer()
    {
        if(_unit.IsDie)
            return;
        _attackTimer += Time.deltaTime;
        if(_unit.Data.AttackSpeed <= _attackTimer)
        {
            _attackTimer = 0;
            _animator.SetTrigger("doAttack");
        }
    }
}