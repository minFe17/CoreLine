using UnityEngine;
using UnityEngine.UI;

public class SettingPopUp : PopUp
{
    private Slider _bgmSlider;
    private Slider _effectSlider;


    protected void Start()
    {
        _bgmSlider = transform.Find("Panel/BGM/BGMSound").GetComponent<Slider>();
        _effectSlider = transform.Find("Panel/Effect/EffectSound").GetComponent<Slider>();
    }
    
}
