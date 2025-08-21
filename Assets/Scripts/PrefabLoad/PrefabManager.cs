using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    Dictionary<EPrefabType, PrefabLoadBase> _prefabDict;

    void SetDictionary()
    {
        _prefabDict = new Dictionary<EPrefabType, PrefabLoadBase>
        {
            {EPrefabType.AttackRange, new AttackRangePrefabLoad() },
            {EPrefabType.Unit, new UnitPrefabLoad() }
        };
    }

    public async Task LoadPrefab()
    {
        SetDictionary();
        foreach (PrefabLoadBase prefabLoad in _prefabDict.Values)
            await prefabLoad.LoadPrefab();
    }

    public PrefabLoadBase GetPrefabLoad(EPrefabType key)
    {
        return _prefabDict[key];
    }

    public bool CheckLoadPrefab()
    {
        if (_prefabDict == null)
            return false;
        if (_prefabDict[EPrefabType.AttackRange].GetPrefab() != null)
            return true;
        return false;
    }
}