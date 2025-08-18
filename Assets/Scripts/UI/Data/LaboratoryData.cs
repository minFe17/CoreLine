using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LaboratoryData
{
    [SerializeField] string _id;
    [SerializeField] string _name;
    [SerializeField] string _type; //enum으로 빼자
    [SerializeField] int _cost;
    [SerializeField] int _value;
    [SerializeField] string _valueType;
    [SerializeField] string _prerequisite;
}