using UnityEngine;
using Utils;

public class JSonTest : MonoBehaviour
{
    [SerializeField] TextAsset unitData;
    [SerializeField] TextAsset fusionUnitData;

    void Awake()
    {
        UnitDataList temp = SimpleSingleton<UnitDataList>.Instance;
        string data = unitData.text;
        JsonUtility.FromJsonOverwrite(data, temp);

        FusionDataList dataList = SimpleSingleton<FusionDataList>.Instance;
        data = fusionUnitData.text;
        JsonUtility.FromJsonOverwrite(data, dataList);
    }
}