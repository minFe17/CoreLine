using UnityEngine;
using System;

public sealed class HealthComponent : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int _maxHp = 100;
    private int _currentHp = 0;
    private bool _isDead = false;

    [Header("Attack")]
    [SerializeField] private int _baseAttack = 10;
    private int _currentAttack = 0;

    public int MaxHp { get { return _maxHp; } }
    public int CurrentHp { get { return _currentHp; } }
    public bool IsDead { get { return _isDead; } }

    public int BaseAttack { get { return _baseAttack; } }
    public int CurrentAttack { get { return _currentAttack; } }

    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    private void OnEnable()
    {
        _isDead = false;
        _currentHp = Mathf.Max(1, _maxHp);
        _currentAttack = Mathf.Max(1, _baseAttack);
        FireHealthChanged();
       
    }

    public void Damage(int amount)
    {
        if (_isDead) { return; }
        int dmg = Mathf.Max(0, amount);
        _currentHp = Mathf.Max(0, _currentHp - dmg);
        FireHealthChanged();
        if (_currentHp == 0)
        {
            _isDead = true;
            if (OnDied != null) { OnDied(); }
        }
    }

    public void Heal(int amount)
    {
        if (_isDead) { return; }
        int heal = Mathf.Max(0, amount);
        _currentHp = Mathf.Min(_maxHp, _currentHp + heal);
        FireHealthChanged();
    }

    public void Kill()
    {
        if (_isDead) { return; }
        _currentHp = 0;
        FireHealthChanged();
        _isDead = true;
        if (OnDied != null) { OnDied(); }
    }

    private void FireHealthChanged()
    {
        if (OnHealthChanged != null) { OnHealthChanged(_currentHp, _maxHp); }
    }

   
}
