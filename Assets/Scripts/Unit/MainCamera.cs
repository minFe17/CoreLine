using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MainCamera : MonoBehaviour
{
    UniversalAdditionalCameraData _cameraData;

    public void Init(Camera fusionCamera)
    {
        _cameraData = GetComponent<UniversalAdditionalCameraData>();
        _cameraData.cameraStack.Add(fusionCamera);
    }
}