using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class SettingPopUp : PopUp
{
    private Slider _bgmSlider;
    private Slider _effectSlider;
    private SoundData _sound;
    string _path;

    protected void Start()
    {
        _bgmSlider = transform.Find("Panel/BGM/BGMSound").GetComponent<Slider>();
        _effectSlider = transform.Find("Panel/Effect/EffectSound").GetComponent<Slider>();
        _sound = DataManager.Instance.GameData.Sound;
        _bgmSlider.value = _sound.BgmVolume;
        _effectSlider.value = _sound.SfxVolume;
    }

    public void ChangeBGMVolume()
    {
        MonoSingleton<AudioClipManager>.Instance.ChangeBGMVolume(_bgmSlider.value);
        _sound.BgmVolume = _bgmSlider.value;
    }

    public void ChangeSFXVolume()
    {
        MonoSingleton<AudioClipManager>.Instance.ChangeSFXVolume(_effectSlider.value);
        _sound.SfxVolume = _effectSlider.value;
    }
}