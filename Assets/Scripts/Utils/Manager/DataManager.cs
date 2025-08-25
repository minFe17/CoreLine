using UnityEngine;
using Utils;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

[JsonConverter(typeof(StringEnumConverter))]
public enum UpgradeType
{
    AttackDamage, AttackRange, HealthPoint
}
public class DataManager : SimpleSingleton<DataManager>
{
    private List<InventoryData> _inventoryDatas;
    private List<LaboratoryData> _laboratoryDatas;
    private List<UpgradeData> _upgradeDatas;
    private GameData _gameData;

    public List<InventoryData> InventoryDatas
    {
        get { return _inventoryDatas; }
    }
    public List<LaboratoryData> LaboratoryDatas
    {
        get { return _laboratoryDatas; }
    }
    public List<UpgradeData> UpgradeDatas
    {
        get { return _upgradeDatas; }
    }
    public GameData GameData
    {
        get { return _gameData; }
    }

    public void LoadData()
    {
        LoadInventoryDatas();
        LoadLaboratoryDatas();
        LoadUpgradeDatas();
        LoadUpgradeData();

    }

    public void SaveData()
    {//데이터 세이브구현

    }
    private void LoadInventoryDatas()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("UI/Data/InventoryData");
        _inventoryDatas = JsonConvert.DeserializeObject<List<InventoryData>>(jsonFile.text);
    }
    private void LoadLaboratoryDatas()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("UI/Data/LaboratoryData");
        _laboratoryDatas = JsonConvert.DeserializeObject<List<LaboratoryData>>(jsonFile.text);
    }
    private void LoadUpgradeDatas()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("UI/Data/UpgradeData");
        _upgradeDatas = JsonConvert.DeserializeObject<List<UpgradeData>>(jsonFile.text);
    }
    private void LoadUpgradeData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("UI/Data/GameData");
        _gameData = JsonConvert.DeserializeObject<GameData>(jsonFile.text);
    }
}
