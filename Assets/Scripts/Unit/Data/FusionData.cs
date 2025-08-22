using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class FusionData
{
    [SerializeField] string _fusionUnitType;
    [SerializeField] List<string> _materialUnit;
    [SerializeField] UnitState _fusionUnitState;

    public EFusionUnitType UnitType { get => (EFusionUnitType)Enum.Parse(typeof(EFusionUnitType), _fusionUnitType); }
    public IReadOnlyList<EUnitType> MaterialUnit
    {
        get => _materialUnit.Select(type => (EUnitType)Enum.Parse(typeof(EUnitType), type)).ToList().AsReadOnly();
    }
}