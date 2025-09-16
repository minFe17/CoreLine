using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public class MapUnitManager
{
    // ╫л╠шео
    Dictionary<Vector3Int, Unit> _unitDict = new Dictionary<Vector3Int, Unit>();

    KingUnit _king;
    int _unitDieCount;

    public KingUnit King { get => _king; set => _king = value; }
    public int UnitDieCount { get => _unitDieCount; }

    public void AddUnit(Vector3Int key, Unit value)
    {
        if(_unitDict.ContainsKey(key))
            return;
        _unitDict[key] = value;
    }

    public Unit GetUnit(Vector3Int key)
    {
        if(_unitDict.TryGetValue(key, out Unit unit))
            return unit;
        return null;
    }

    public void RemoveUnit(Vector3Int key)
    {
        _unitDict.Remove(key);
    }

    public void AddDieUnit()
    {
        _unitDieCount++;
    }

    public void RestartGame()
    {
        _unitDieCount = 0;
        SimpleSingleton<BombManager>.Instance.RemoveBomb();

        foreach (Unit unit in _unitDict.Values.ToList())
            unit.Remove();
        _unitDict.Clear();

        if (_king == null)
            return;
        _king.Remove();
        _king = null;
        SimpleSingleton<FusionManager>.Instance.ResetCost();
    }
}