using UnityEngine;
using Utils;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
public class DataManager : SimpleSingleton<DataManager>
{
    private List<InventoryData> _inventoryDatas;
    private List<LaboratoryData> _laboratoryDatas;
    private List<UpgradeData> _upgradeDatas;
    private List<WorldStageData> _worldStagesDatas;
    private List<StoreData> _storeDatas;
    private List<SkillData> _skillDatas;
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
    public List<WorldStageData> WorldStageDatas
    {
        get { return _worldStagesDatas; }
    }
    public List<StoreData> StoreDatas
    {
        get { return _storeDatas; } 
    }
    public List<SkillData> SkillDatas
    {
        get { return _skillDatas; }
    }

    public GameData GameData
    {
        get { return _gameData; }
        set { SetGameData(value); }  
    }

    public void LoadData()
    {
        LoadInventoryDatas();
        LoadLaboratoryDatas();
        LoadUpgradeDatas();
        LoadNormalStageData();
        LoadStoreData();
        LoadSkillData();
    }
    public UpgradeData GetUpgradeData(UpgradeType type)
    {
        foreach (UpgradeData data in _upgradeDatas) 
        {
            if (data.UpgradeType == type)
                return data;
        }
        return new UpgradeData();
    }
    public List<NormalStageData> GetStages(StageType type)
    {
        foreach(var world in _worldStagesDatas)
        {
            if(world.StageType == type)
                return world.Stages;
        }
        return null;
    }

    public void SaveData()
    {//데이터 세이브구현
        FireBaseManager.Instance.SaveGameData(_gameData);
    }
    private void SetGameData(GameData data)
    {
        if (data.UnlockedLaboratoryId == null)
            data.UnlockedLaboratoryId = new();
        if (data.ClearStage == null)
            data.ClearStage = new();
        if (data.UnlockedUnit == null)
            data.UnlockedUnit = new();
        if(data.Sound == null)
            data.Sound = new();

        _gameData = data;
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
    private void LoadGameData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("UI/Data/GameData");
        _gameData = JsonConvert.DeserializeObject<GameData>(jsonFile.text);
    }
    private void LoadNormalStageData()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("UI/Data/NormalStageData");
        _worldStagesDatas = JsonConvert.DeserializeObject<List<WorldStageData>>(jsonFile.text);
    }
    private void LoadStoreData()
    {
        _storeDatas = CsvManager.Instance.Load<StoreData>("StoreData");
    }
    private void LoadSkillData()
    {
        _skillDatas = CsvManager.Instance.Load<SkillData>("SkillData");
    }
}
