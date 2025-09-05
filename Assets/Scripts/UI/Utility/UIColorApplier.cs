using UnityEngine;
using UnityEngine.UI;
using System;


public enum ColorType
{
    Dark, Normal, Light
}
public class UIColorApplier : MonoBehaviour
{

    [SerializeField]
    private ColorType _colorType;

    //private static StageType _stageType = StageType.Infinity;
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
        MatchColor(UIGameManager.Instance.StageType);
        EventManager.Instance.Subscribe<StageType>("ChangeStage", MatchColor);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("ChangeStage", (Action<StageType>)MatchColor);
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
    private void MatchColor(StageType type) //이거 stage변경될때만 실행되게 짜야됨
    {
        switch (type)
        {
            case StageType.Infinity:
                RGBToHSLUtils.Color = new Color(0.6f, 0.4f, 0.8f);
                break;
            case StageType.Stage1:
                RGBToHSLUtils.Color = new Color(1f, 0.95f, 0.4f);
                break;
            case StageType.Stage2:
                RGBToHSLUtils.Color = new Color(0.85f, 0.3f, 0.2f);
                break;
            case StageType.Stage3:
                RGBToHSLUtils.Color = new Color(0.2f, 0.6f, 0.6f);
                break;
        }

        SettingColor();
    }
}
