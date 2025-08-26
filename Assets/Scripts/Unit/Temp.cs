using UnityEngine;
using Utils;

public class Temp : MonoBehaviour
{
    async void Start()
    {
        if(!SimpleSingleton<PrefabManager>.Instance.CheckLoadPrefab())
            await SimpleSingleton<PrefabManager>.Instance.LoadPrefab();
        SimpleSingleton<FactoryManager>.Instance.Init();
        Debug.Log(1);
        MonoSingleton<GameStateManager>.Instance.SetState();
    }
}