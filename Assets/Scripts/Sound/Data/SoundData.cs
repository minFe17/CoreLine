using UnityEngine;

[System.Serializable]
public class SoundData
{
    [SerializeField] float _bgmVolume = 0.5f;
    [SerializeField] float _sfxVolume = 0.5f;

    public float BgmVolume { get => _bgmVolume; set => _bgmVolume = value; }
    public float SfxVolume { get => _sfxVolume; set => _sfxVolume = value; }
}