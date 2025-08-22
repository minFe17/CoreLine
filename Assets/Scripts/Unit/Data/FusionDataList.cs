using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FusionDataList
{
    [SerializeField] List<FusionData> _fusionDataList;

    public IReadOnlyList<FusionData> DataList => _fusionDataList;
}