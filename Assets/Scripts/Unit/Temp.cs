using UnityEngine;
using Utils;

public class Temp : MonoBehaviour
{
    async void Start()
    {
        if(!SimpleSingleton<PrefabManager>.Instance.CheckLoadPrefab())
            await SimpleSingleton<PrefabManager>.Instance.LoadPrefab();
        SimpleSingleton<FactoryManager>.Instance.Init();
    }
}