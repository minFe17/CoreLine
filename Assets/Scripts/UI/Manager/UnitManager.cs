using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using Utils;



public class UnitManager : SimpleSingleton<UnitManager>
{
    private List<InventoryData> _inventoryDatas;
    private List<UpgradeData> _upgradeDatas;
    private GameData _gameData;
    private Dictionary<EUnitType, UnlockedUnit> _settingUnits = new Dictionary<EUnitType, UnlockedUnit>();
    private List<UnlockedUnit> _unlockedUnits;
    private InventoryData _choiceUnit = new InventoryData();

    public UnitManager()
    {
        SettingData();
        EventManager.Instance.Subscribe<EUnitType>("ChangeChoiceUnitData", SettingChoiceUnit);
        EventManager.Instance.Subscribe("AddSelectedUnit", AddSelectedUnit);
        EventManager.Instance.Subscribe("DeleteSelectedUnit", DeleteSelectedUnit); 
        EventManager.Instance.Subscribe("Reset", ResetChoice);
        EventManager.Instance.Subscribe("BuyUnit", BuyUnit);
        EventManager.Instance.Subscribe<UpgradeType>("UpgradeUnit", UpgradeUnit);
    }
    public InventoryData ChoiceUnit
    {
        get { return _choiceUnit; }
    }
    public Dictionary<EUnitType, UnlockedUnit> SettingUnits
    {
        get { return _settingUnits; }
    }
    public List<UnlockedUnit>UnlockedUnits
    { 
        get { return _unlockedUnits; } 
    }
    public bool IsSelected()
    {
        return _settingUnits.ContainsKey(_choiceUnit.UnitType);
    }

    
    public InventoryData GetInventoryData(EUnitType type)
    {
        return _inventoryDatas.Find(data => data.UnitType == type);
    }
    public int AllUnitCount()
    {
        return _inventoryDatas.Count;
    }

    public bool IsGetUnit(EUnitType type)
    {
        foreach (UnlockedUnit unit in _unlockedUnits)
        {
            if (unit.UnitType == type)
                return true;
        }
        return false;
    }
    private void UpgradeUnit(UpgradeType type)
    {
        UnlockedUnit findUnit = _unlockedUnits.FirstOrDefault(unit => unit.UnitType == _choiceUnit.UnitType);
        if (findUnit == null)
        {
            Debug.Log("유닛못찾음 오류");
            return;
        }
        Debug.Log(findUnit.UnitType);
        switch (type)
        {
            case UpgradeType.AttackDamage:
                findUnit.AttackDatamageLevel++;
                Debug.Log("damage++");
                break;
            case UpgradeType.AttackRange:
                findUnit.AttackRangeLevel++;
                Debug.Log("Range++");
                break;
            case UpgradeType.HealthPoint:
                findUnit.HealthPointLevel++;
                Debug.Log("healthPoint++");
                break;
        }
    }
    private void BuyUnit()
    {
        UnlockedUnit unit = new UnlockedUnit();
        unit.UnitType = _choiceUnit.UnitType;

        _unlockedUnits.Add(unit);

        EventManager.Instance.Invoke("SettingBuyUnit");
    }
    private void SettingChoiceUnit(EUnitType param)
    {
        Debug.Log(param);
        _choiceUnit = GetInventoryData(param);
    }
    private void SettingData()
    {
        _inventoryDatas = DataManager.Instance.InventoryDatas;
        _upgradeDatas = DataManager.Instance.UpgradeDatas;
        _gameData = DataManager.Instance.GameData;
        _unlockedUnits = _gameData.UnlockedUnit;
    }
    private void ResetChoice()
    {
        _choiceUnit = default;
    }
    private void AddSelectedUnit()
    {
        if (_settingUnits.Count >= 8) return;
        // 이미 키가 있는지 확인
        if (_settingUnits.ContainsKey(_choiceUnit.UnitType)) return;
        // 팝패널띄우기

        // 리스트에서 해당 타입을 가진 유닛 찾기
        UnlockedUnit foundUnit = _unlockedUnits.Find(unit => unit.UnitType == _choiceUnit.UnitType);
        if (foundUnit != null)
        {
            _settingUnits.Add(_choiceUnit.UnitType, foundUnit);
        }
    }
    private void DeleteSelectedUnit()
    {
        if (_settingUnits.Count <= 0) return;
        //뺄거있는지 확인
        if (!_settingUnits.ContainsKey(_choiceUnit.UnitType)) return; //팝패널 띄우기

        _settingUnits.Remove(_choiceUnit.UnitType);
    }
}
