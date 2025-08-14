using UnityEngine;

[System.Serializable]
public class UnitLevelData
{
    [SerializeField] int _hp;
    [SerializeField] int _attackDamage;
    [SerializeField] float _attackSpeed;
    [SerializeField] float _attackRange;
    [SerializeField] int _cost;

    public int HP { get => _hp; }
    public int AttackDamage { get => _attackDamage; }
    public float AttackSpeed { get => _attackSpeed; }
    public float AttackRange { get=> _attackRange; }
    public int Cost { get => _cost; }
}