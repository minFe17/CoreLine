using UnityEngine;

[System.Serializable]
public class UnitLevelData
{
    [SerializeField] int _cost;
    [SerializeField] UnitState _unitState;

    public int Cost { get => _cost; }
    public UnitState UnitState { get => _unitState; }
}