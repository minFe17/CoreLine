using UnityEngine;
using UnityEngine.Rendering;
using Utils;

public class FusionPostVolume : MonoBehaviour, IMediatorEvent
{
    Volume _fusionVolume;

    void Start()
    {
        _fusionVolume = GetComponent<Volume>();
        _fusionVolume.enabled = false;
        SimpleSingleton<MediatorManager>.Instance.Register(EMediatorType.Fusion, this);
    }

    void IMediatorEvent.HandleEvent(object data)
    {
        bool value = (bool)data;
        _fusionVolume.enabled = value;
    }
}