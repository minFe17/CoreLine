using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using UnityEngine;
using JetBrains.Annotations;


[JsonConverter(typeof(StringEnumConverter))]
public enum ClearType
{
   MoneySave, HealthSave, UnitSave
}
[System.Serializable]
public struct NormalStageData
{
    public string Id;
    public string Name;
    public List<Condition> Condition;
    public int Gold;
    public int Gem;
    public string UnlockCharacter;
}
[System.Serializable]
public struct Condition
{
    public ClearType ClearType;
    public string Info;           
    public float Value;           
}