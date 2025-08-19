using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LaboratoryData
{
    public string Id;
    public string Name;
    public string Type; //enum으로 빼자
    public int Cost;
    public int Value;
    public string ValueType;
    public string Prerequisite;
}