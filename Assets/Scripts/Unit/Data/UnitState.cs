using UnityEngine;

[System.Serializable]
public class UnitState
{
    [SerializeField] int _hp;
    [SerializeField] int _attackDamage;
    [SerializeField] float _attackSpeed;
    [SerializeField] float _attackRange;

    int _currentAttackDamage;
    public int HP { get => _hp; }
    public int AttackDamage
    {
        get
        {
            if (_currentAttackDamage == 0)
                _currentAttackDamage = _attackDamage;
            return _currentAttackDamage;
        }
    }
    public float AttackSpeed { get => _attackSpeed; }
    public float AttackRange { get => _attackRange; }

    public void AddAttackDamage(int damage)
    {
        _currentAttackDamage += damage;
    }

    public void RestorationAttackDamage()
    {
        _currentAttackDamage = _attackDamage;
    }
}