using UnityEngine;
using Utils;
using System.Collections.Generic;

public class DataManager : SimpleSingleton<DataManager>
{
    private List<InventoryData> _inventoryDatas;
    private List<LaboratoryData> _laboratoryDatas;
    private List<UpgradeData> _upgradeDatas;
    private GameData _gameData;

    public void LoadData()
    {
        LoadInventoryDatas();
    }
    public void UpdateData()
    {//세이브데이타 업데이트해주기

    }
    public void SaveData()
    {//데이터 세이브구현

    }
    private void LoadInventoryDatas()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("UI/Data/InventoryData");
        InventoryData[] array = JsonHelper.FromJson<InventoryData>(jsonFile.text);
        _inventoryDatas = new List<InventoryData>(array);
    }
    private void LoadLaboratoryDatas()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("UI/Data/LaboratoryData");
        LaboratoryData[] array = JsonHelper.FromJson<LaboratoryData>(jsonFile.text);
        _laboratoryDatas = new List<LaboratoryData>(array);
    }
    private void LoadUpgradeDatas()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("UI/Data/UpgradeData");
        UpgradeData[] array = JsonHelper.FromJson<UpgradeData>(jsonFile.text);
        _upgradeDatas = new List<UpgradeData>(array);
    }
    private void LoadUpgradeData()
    {
    }
}
