using UnityEditor.Build.Pipeline.Tasks;
using UnityEngine;
using UnityEngine.UI;

public enum StageType
{
    Stage1, Stage2
}//이거 스테이지 나오는거 보고 맞춰야됨
public class UIColorApplier : MonoBehaviour
{
    private enum ColorType
    {
        Dark, Normal,Light
    }

    [SerializeField]
    private ColorType _colorType;

    private static StageType _stageType = StageType.Stage1;
    private Color _color;
    private Image _image;


    private void Awake()
    {
        _image = GetComponent<Image>();
    }
    private void OnEnable()
    {
        MatchColor();
        SettingColor();
    }

    private void SettingColor()
    {
        switch (_colorType)
        {
            case ColorType.Dark:
                _color = RGBToHSLUtils.Darken(RGBToHSLUtils.Color, 0.3f);
                break;
            case ColorType.Normal:
                _color = RGBToHSLUtils.Color;
                break;
            case ColorType.Light:
                _color = RGBToHSLUtils.Lighten(RGBToHSLUtils.Color, 0.3f);
                break;
        }
        _image.color = _color;
    }
    private void MatchColor() //이거 stage변경될때만 실행되게 짜야됨
    {
        switch (_stageType)
        {
            case StageType.Stage1:
                RGBToHSLUtils.Color = new Color(1f, 0.95f, 0.4f, 1f);
                break;
            case StageType.Stage2:
                RGBToHSLUtils.Color = Color.red;
                break;
        }
    }
}
