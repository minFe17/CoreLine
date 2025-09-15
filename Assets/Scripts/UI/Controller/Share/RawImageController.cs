using UnityEngine;
using UnityEngine.UI;

public class RawImageController : MonoBehaviour
{
    private enum TextureType
    {
        BackGroundTexture,UnitTexture
    }
    [SerializeField]
    private TextureType _type;
    private RawImage _screen;

    private void Start()
    {
        //¿©±â Â¥
    }
}