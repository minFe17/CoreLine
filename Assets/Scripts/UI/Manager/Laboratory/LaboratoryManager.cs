using UnityEngine;
using Utils;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;

[System.Serializable]
public class UsingLaboratoryData
{
    public Dictionary<TargetType, int> Shield = new(); //왕, 유닛 적용
    public int Heal = new(); //왕 적용 유닛 고민
    public float AttackDamage = new (); //유닛 적용
    public float AttackSpeed = new(); // 유닛 적용
    public float GetMoney = new(); //유틸리티 적용
    public float GetGem = new(); //유틸리티 적용
    public int SubPlayTime = new(); //유틸리티 적용
    public List<string> UnlockedSkill = new();
}

public class LaboratoryManager : SimpleSingleton<LaboratoryManager>
{
    private Dictionary<LaboratoryType,List<LaboratoryData>> _data = new Dictionary<LaboratoryType, List<LaboratoryData>>();

    public LaboratoryManager()
    {
        SettingData();
    }
    public List<LaboratoryData> GetData(LaboratoryType type)
    {
        return _data[type];
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

}
