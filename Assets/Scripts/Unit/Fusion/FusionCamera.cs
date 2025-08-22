using UnityEngine;
using Utils;

public class FusionCamera : MonoBehaviour, IMediatorEvent
{
    Camera _fusionCamera;

    void Start()
    {
        _fusionCamera = GetComponent<Camera>();
        SimpleSingleton<MediatorManager>.Instance.Register(EMediatorType.Fusion, this);
    }

    void IMediatorEvent.HandleEvent(object data)
    {
        _fusionCamera.enabled = (bool)data;
    }
}
