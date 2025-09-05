using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Utils;
using static UnityEngine.Rendering.DebugUI;



[System.Serializable]
public class UsingLaboratoryData
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
    public UsingLaboratoryData UsingLabData
    {
        get { return _usingLabData; }
    }
    public List<string> UnlockedSkill
    {
        get { return _usingLabData.UnlockedSkill; }
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
    public LaboratoryData GetBuyLaboratoryData(string id)
    {
        return _buyLaboratory[id];
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

        List<string> unlocked = DataManager.Instance.GameData.UnlockedLaboratoryId;
        foreach (string id in unlocked)
        {
            foreach (LaboratoryData dt in DataManager.Instance.LaboratoryDatas)
            {
                if (dt.Id == id)
                {
                    _buyLaboratory.Add(id, dt);
                    SettingValue(dt);
                    break;
                }
            }
        }
    }
    private void ChoiceLaboratoryData(LaboratoryData data)
    {
        _choiceLaboratory = data;
    }
    private void BuyLaboratory(LaboratoryData data)
    {
        if (_buyLaboratory.ContainsKey(data.Id)) return; //ÆÐ³Î¶ç¿ì±â
        _buyLaboratory.Add(data.Id, data);

        DataManager.Instance.GameData.UnlockedLaboratoryId.Add(data.Id);

        SettingValue(data);
    }
    private void SettingValue(LaboratoryData data)
    {

        if (data.Effect.TargetStatus == TargetStatus.Skill)
        {
            _usingLabData.UnlockedSkill.Add(data.Id);
            return;
        }

        switch (data.Effect.TargetType)
        {
            case TargetType.King:
                _usingLabData.King.ApplyEffect(data);
                break;
            case TargetType.Unit:
                _usingLabData.Unit.ApplyEffect(data);
                break;
            default:
                _usingLabData.Utility.ApplyEffect(data);
                break;
        }
    }
    
}
