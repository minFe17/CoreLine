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
        _screen = GetComponent<RawImage>();
        _screen.texture = Resources.Load<RenderTexture>("UI/Textures/" + _type.ToString());
    }
}