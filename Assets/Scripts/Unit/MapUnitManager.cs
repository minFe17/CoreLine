using System.Collections.Generic;
using UnityEngine;

public class MapUnitManager : MonoBehaviour
{
    Dictionary<Vector3Int, Unit> _unitDict = new Dictionary<Vector3Int, Unit>();

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
}