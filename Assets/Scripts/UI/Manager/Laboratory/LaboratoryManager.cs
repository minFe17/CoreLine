using UnityEngine;
using Utils;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using Unity.VisualScripting;

public class LabData { }
[System.Serializable]
public class UnitLabData:LabData
{
    public Pair<ValueType, float> AttackDamage = new();
    public Pair<ValueType, float> AttackSpeed = new();
}

[System.Serializable]
public class KingLabData : LabData
{
    public Pair<ValueType, int> Heal = new();
    public Pair<ValueType, int> Shield = new();
}

[System.Serializable]
public class UtilityLabData : LabData
{
    public Pair<ValueType, float> GetMoney = new();
    public Pair<ValueType, float> GetGem = new();
    public Pair<ValueType, int> SubPlayTime = new();
}

[System.Serializable]
public class UsingLaboratoryData : LabData
{
    public KingLabData King = new();
    public UnitLabData Unit = new();
    public UtilityLabData Utility = new();
    public List<string> UnlockedSkill = new();
}

public class LaboratoryManager : SimpleSingleton<LaboratoryManager>
{
    private Dictionary<LaboratoryType,List<LaboratoryData>> _data = new Dictionary<LaboratoryType, List<LaboratoryData>>();
    private Dictionary<string, LaboratoryData> _buyLaboratory = new();
    private LaboratoryData _choiceLaboratory = new();
    private UsingLaboratoryData _usingLabData = new UsingLaboratoryData();

    public LaboratoryManager()
    {
        SettingData();
        EventManager.Instance.Subscribe<LaboratoryData>("ChangeChoiceLaboratory", ChoiceLaboratoryData);
        EventManager.Instance.Subscribe<LaboratoryData>("BuyLaboratory", BuyLaboratory);
    }
    public List<LaboratoryData> GetData(LaboratoryType type)
    {
        return _data[type];
    }
    public LaboratoryData ChoiceLaboratory
    {
        get { return _choiceLaboratory; }
    }
    public Dictionary<string,LaboratoryData> BuyLaboratoryData
    {
        get { return _buyLaboratory; }
    }
    private void SettingData()
    {
        _data[LaboratoryType.Attack] = new List<LaboratoryData>();
        _data[LaboratoryType.Defense] = new List<LaboratoryData>();
        _data[LaboratoryType.Utility] = new List<LaboratoryData>();
        List<LaboratoryData> data = DataManager.Instance.LaboratoryDatas;

        foreach(LaboratoryData dt in data)
        {
            switch(dt.LaboratoryType)
            {
                case LaboratoryType.Attack:
                    _data[LaboratoryType.Attack].Add(dt);
                    break;
                case LaboratoryType.Defense:
                    _data[LaboratoryType.Defense].Add(dt);
                    break;
                case LaboratoryType.Utility:
                    _data[LaboratoryType.Utility].Add(dt);
                    break;
            }
        }
    }
    private void ChoiceLaboratoryData(LaboratoryData data)
    {
        _choiceLaboratory = data;
    }
    private void BuyLaboratory(LaboratoryData data)
    {
        if (_buyLaboratory.ContainsKey(data.Id)) return; //패널띄우기
        _buyLaboratory.Add(data.Id, data);
        DataManager.Instance.GameData.UnlockedLaboratoryId.Add(data.Id);

        SettingValue(ref data);
    }
    private void SettingValue(ref LaboratoryData data)
    {
        LabData labData = null;
        switch (data.Effect.TargetType)
        {
            case TargetType.King:
                labData = new KingLabData();
                break;
            case TargetType.Unit: 
                labData = new UnitLabData();
                break;
            case TargetType.Monster:
                break;
            default:
                labData = new UtilityLabData();
                break;
        }
        if (data.Effect.ValueType == ValueType.Skill)
        {
            //스킬넣어주기
            return;
        }

        //여기짜기

    }
    
}
