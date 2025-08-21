using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Utils;

public class UnitManager : SimpleSingleton<UnitManager>
{
    private List<InventoryData> _inventoryDatas;
    private List<UpgradeData> _upgradeDatas;
    private GameData _gameData;
    private List<UnlockedUnit> _settingUnits = new List<UnlockedUnit>();
    private List<UnlockedUnit> _unlockedUnits;
    private InventoryData _choiceUnit = new InventoryData();

    public InventoryData ChoiceUnit
    {
        get { return _choiceUnit; }
        set { _choiceUnit = value; }
    }
    public List<UnlockedUnit> SettingUnits
    {
        get { return _settingUnits; }
    }
    public List<UnlockedUnit>UnlockedUnits
    { 
        get { return _unlockedUnits; } 
    }
    public void SettingData()
    {
        _inventoryDatas = DataManager.Instance.InventoryDatas;
        _upgradeDatas = DataManager.Instance.UpgradeDatas;
        _gameData = DataManager.Instance.GameData;
        _unlockedUnits = _gameData.UnlockedUnit;
    }
    public InventoryData GetInventoryData(string type)
    {
        return _inventoryDatas.Find(data => data.UnitType == type);
    }
    public int AllUnitCount()
    {
        return _inventoryDatas.Count;
    }
    public void BuyUnit(string unitType)
    {
        UnlockedUnit unit = new UnlockedUnit();
        unit.Type = unitType;

        _unlockedUnits.Add(unit);
    }
    public bool IsGetUnit(string type)
    {
        foreach (UnlockedUnit unit in _unlockedUnits)
        {
            if (unit.Type == type)
                return true;
        }
        return false;
    }

}
