using NaughtyAttributes;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] protected EUnitType _unitType;
    [ShowIf("IsNotKing")]
    [SerializeField] protected int _level;
    
    protected UnitLevelData _data;

    #region NaughtyAttributes
    bool IsNotKing() => _unitType != EUnitType.King;
    #endregion
}