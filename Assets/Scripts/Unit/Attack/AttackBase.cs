using UnityEngine;

public abstract class AttackBase : MonoBehaviour
{
    [SerializeField] protected EUnitType _unitType;

    protected abstract void Attack();
    bool _isDie;

    void OnEnable()
    {
        _isDie = false;
    }

    void Update()
    {
        AttackTimer();
    }

    void AttackTimer()
    {
        if(_isDie)
            return;
    }

    #region Animation Event
    public void OnDieEvent()
    {
        _isDie = true;
    }
    #endregion
}