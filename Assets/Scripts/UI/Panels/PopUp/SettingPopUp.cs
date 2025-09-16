using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class SettingPopUp : PopUp
{
    private Slider _bgmSlider;
    private Slider _effectSlider;

    SoundData _soundData;
    string _path;

    protected void Start()
    {
        _bgmSlider = transform.Find("Panel/BGM/BGMSound").GetComponent<Slider>();
        _effectSlider = transform.Find("Panel/Effect/EffectSound").GetComponent<Slider>();

        ReadSoundData();
    }

    void ReadSoundData()
    {
        if(_soundData == null)
            _soundData = SimpleSingleton<SoundData>.Instance;
        if(_path == null)
            _path = Application.persistentDataPath + "SaveSoundDataFile.json";

        if (File.Exists(_path))
        {
            string json = File.ReadAllText(_path);
            JsonUtility.FromJsonOverwrite(json, _soundData);
        }
        
        _bgmSlider.value = _soundData.BgmVolume;
        _effectSlider.value = _soundData.SfxVolume;
        ChangeBGMVolume();
        ChangeSFXVolume();
    }

    void WriteSoundData()
    {
        string json = JsonUtility.ToJson(_soundData, true);
        File.WriteAllText(_path, json);
    }

    public void ChangeBGMVolume()
    {
        MonoSingleton<AudioClipManager>.Instance.ChangeBGMVolume(_bgmSlider.value);
        _soundData.BgmVolume = _bgmSlider.value;
        WriteSoundData();
    }

    public void ChangeSFXVolume()
    {
        MonoSingleton<AudioClipManager>.Instance.ChangeSFXVolume(_effectSlider.value);
        _soundData.SfxVolume = _effectSlider.value;
        WriteSoundData();
    }
}