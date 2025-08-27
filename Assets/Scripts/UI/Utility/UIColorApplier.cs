using UnityEditor.Build.Pipeline.Tasks;
using UnityEngine;
using UnityEngine.UI;

public enum StageType
{
    Stage1, Stage2
}//이거 스테이지 나오는거 보고 맞춰야됨
public enum ColorType
{
    Dark, Normal, Light
}
public class UIColorApplier : MonoBehaviour
{

    [SerializeField]
    private ColorType _colorType;

    private static StageType _stageType = StageType.Stage1;
    private Color _color;
    private Image _image;


    public ColorType MyColorType
    {
        get { return _colorType; }
        set 
        {
            if (_colorType == value) return;
            _colorType = value;
            SettingColor();
        }
    }
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
                _color = RGBToHSLUtils.Lighten(RGBToHSLUtils.Color, 0.2f);
                break;
        }
        _image.color = _color;
    }
    private void MatchColor() //이거 stage변경될때만 실행되게 짜야됨
    {
        switch (_stageType)
        {
            case StageType.Stage1:
                RGBToHSLUtils.Color = new Color(1f, 0.95f, 0.4f);
                break;
            case StageType.Stage2:
                RGBToHSLUtils.Color = new Color(0.85f, 0.3f, 0.2f);
                break;
        }
    }
}
