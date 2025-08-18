using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryData
{
    [SerializeField] string _unitType;
    [SerializeField] int _unlockPrice;
    [SerializeField] bool _isUnlocked;
}