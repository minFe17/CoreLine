using UnityEngine;

public class DieEvent : MonoBehaviour
{
    Unit _unit;

    public void Init(Unit unit)
    {
        _unit = unit;
    }

    #region Animation Event
    public void Die()
    {
        _unit.Die();
    }
    #endregion
}