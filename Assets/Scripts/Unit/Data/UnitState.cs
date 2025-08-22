using UnityEngine;

[System.Serializable]
public class UnitState
{
    [SerializeField] int _hp;
    [SerializeField] int _attackDamage;
    [SerializeField] float _attackSpeed;
    [SerializeField] float _attackRange;

    public int HP { get => _hp; }
    public int AttackDamage { get => _attackDamage; }
    public float AttackSpeed { get => _attackSpeed; }
    public float AttackRange { get => _attackRange; }
}