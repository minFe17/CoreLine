using UnityEngine;
using Utils;

public class AudioClipManager :MonoBehaviour ,IMediatorEvent
{
    SoundController _soundController;
    GameObject _soundControllerPrefab;
    PrefabLoadBase _prefabLoad;

    public void Init()
    {
        if (_prefabLoad == null)
            _prefabLoad = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.Sound);
        CreateSoundController();
        SimpleSingleton<MediatorManager>.Instance.Register(EMediatorType.PlayAudio, this);
    }

    void CreateSoundController()
    {
        if (_soundController == null)
        {
            _soundControllerPrefab = _prefabLoad.GetPrefab();
            _soundController = Instantiate(_soundControllerPrefab, transform).GetComponent<SoundController>();
        }
    }

    public void PlayBGM(EBGMType type)
    {
        _soundController.StartBGM(_prefabLoad.GetAudioPrefab(type));
    }

    public void PlaySFX(ESFXType type)
    {
        _soundController.PlaySFXAudio(_prefabLoad.GetAudioPrefab(type));
    }

    public void StopSFX()
    {
        _soundController.StopSFXAudio();
    }

    public void StopBGM()
    {
        _soundController.StopBGM();
    }

    public void ChangeBGMVolume(float value)
    {
        _soundController.ChangeBGMVolume(value);
    }

    public void ChangeSFXVolume(float value)
    {
        _soundController.ChangeSFXVolume(value);
    }

    void IMediatorEvent.HandleEvent(object data)
    {
        if (data is ESFXType sfxType)
            PlaySFX(sfxType);
        else if (data is EBGMType bgmType)
            PlayBGM(bgmType);
    }
}