using UnityEngine;

public class AttackEvent : MonoBehaviour
{
    AttackBase _attackBase;

    public void Init(AttackBase attackBase)
    {
        _attackBase = attackBase;
    }

    #region Animation Event
    public void Attack()
    {
        _attackBase.Attack();
    }
    #endregion
}