using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SoundPrefabLoad : PrefabLoadBase
{
    GameObject _soundControllerPrefab;

    Dictionary<EBGMType, AudioClip> _bgmDict = new Dictionary<EBGMType, AudioClip>();
    Dictionary<ESFXType, AudioClip> _sfxDict = new Dictionary<ESFXType, AudioClip>();

    public override async Task LoadPrefab()
    {
        if (_addressableManager == null)
            Init();

        _soundControllerPrefab = await _addressableManager.GetAddressableAsset<GameObject>("SoundController");

        for (int i = 0; i < (int)EBGMType.Max; i++)
        {
            AudioClip prefab = await _addressableManager.GetAddressableAsset<AudioClip>($"{(EBGMType)i}");
            if (prefab != null && !_bgmDict.ContainsKey((EBGMType)i))
                _bgmDict.Add((EBGMType)i, prefab);
        }

        for (int i = 0; i < (int)ESFXType.Max; i++)
        {
            AudioClip prefab = await _addressableManager.GetAddressableAsset<AudioClip>($"{(ESFXType)i}");
            if (prefab != null && !_sfxDict.ContainsKey((ESFXType)i))
                _sfxDict.Add((ESFXType)i, prefab);
        }
    }

    public override GameObject GetPrefab()
    {
        return _soundControllerPrefab;
    }

    public override AudioClip GetAudioPrefab<TEnum>(TEnum type)
    {
        if (type is EBGMType bgmType)
            return _bgmDict[bgmType];

        if (type is ESFXType sfxType)
            return _sfxDict[sfxType];

        return null;
    }
}