using UnityEngine;
using Utils;
using System.Collections.Generic;
using UnityEngine.Rendering;

public class LaboratoryManager : SimpleSingleton<LaboratoryManager>
{
    private Dictionary<LaboratoryType,List<LaboratoryData>> _data = new Dictionary<LaboratoryType, List<LaboratoryData>>();

    public LaboratoryManager()
    {
        SettingData();
    }
    private void SettingData()
    {
        _data[LaboratoryType.Attack] = new List<LaboratoryData>();
        _data[LaboratoryType.Defense] = new List<LaboratoryData>();
        _data[LaboratoryType.Utility] = new List<LaboratoryData>();
        List<LaboratoryData> data = DataManager.Instance.LaboratoryDatas;

        foreach(LaboratoryData dt in data)
        {
            switch(dt.Type)
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
