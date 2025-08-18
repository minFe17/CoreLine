using UnityEngine;
using Utils;

public class JSonTest : MonoBehaviour
{
    [SerializeField] TextAsset unitData;

    void Awake()
    {
        UnitDataList temp = SimpleSingleton<UnitDataList>.Instance;
        string data = unitData.text;
        JsonUtility.FromJsonOverwrite(data, temp);
    }
}