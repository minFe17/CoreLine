using UnityEngine;
using Utils;
using System.Threading.Tasks;
using System.IO;

public class Lobby : MonoBehaviour
{
    PrefabManager _prefabManager;
    SoundData _soundData;
    string _soundDataPath;
    private bool _isSetting = false;

    public bool IsSetting
    { get { return _isSetting; } }
    public async Task InitializeAsync()
    {
        if (_prefabManager == null)
            _prefabManager = SimpleSingleton<PrefabManager>.Instance;

        if (!_prefabManager.CheckLoadPrefab())
            await _prefabManager.LoadPrefab();

        ReadJson();
        MonoSingleton<AudioClipManager>.Instance.Init();
        SettingSoundVolume();
    }

    void ReadJson()
    {
        ReadData<UnitDataList>(EDataType.UnitData);
        ReadData<FusionDataList>(EDataType.FusionUnitData);

        _isSetting = true;
    }

    void ReadData<T>(EDataType type) where T : new()
    {
        TextAsset data = _prefabManager.GetPrefabLoad(EPrefabType.Data).GetPrefabTextAsset(type);
        string json = data.text;
        T target = SimpleSingleton<T>.Instance;
        JsonUtility.FromJsonOverwrite(json, target);
    }

    void SettingSoundVolume()
    {
        MonoSingleton<AudioClipManager>.Instance.Init();
        ReadSoundData();

        MonoSingleton<AudioClipManager>.Instance.ChangeBGMVolume(_soundData.BgmVolume);
        MonoSingleton<AudioClipManager>.Instance.ChangeSFXVolume(_soundData.SfxVolume);
    }

    void ReadSoundData()
    {
        if (_soundData == null)
            _soundData = SimpleSingleton<SoundData>.Instance;
        if (_soundDataPath == null)
            _soundDataPath = Path.Combine(Application.persistentDataPath, "SaveSoundDataFile.json");

        if (!File.Exists(_soundDataPath))
            return;
        string json = File.ReadAllText(_soundDataPath);
        JsonUtility.FromJsonOverwrite(json, _soundData);
    }


}