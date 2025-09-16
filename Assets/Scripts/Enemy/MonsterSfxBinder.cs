using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utils;

public sealed class MonsterSfxBinder : MonoBehaviour
{
    [SerializeField] private string _saveFileName = "SaveSoundDataFile.json";

    private static readonly string[] _monsterTags = new string[] { "Monster", "Boss" };
    private const float _rescanIntervalSeconds = 0.5f;

    private readonly HashSet<AudioSource> _tracked = new HashSet<AudioSource>();
    private readonly Dictionary<AudioSource, float> _baseVolume = new Dictionary<AudioSource, float>();

    private SoundData _soundData;
    private float _lastApplied = -1f;
    private int _lastAppliedTrackedCount = -1;
    private Coroutine _loopCo;

    private void OnEnable()
    {
        _soundData = SimpleSingleton<SoundData>.Instance;
        LoadSavedSfxVolume();                 
        RefreshTrackedSources();
        ApplyVolumeNow();                    
        _loopCo = StartCoroutine(Loop());
    }

    private void OnDisable()
    {
        if (_loopCo != null)
        {
            StopCoroutine(_loopCo);
            _loopCo = null;
        }
    }

    private void LoadSavedSfxVolume()
    {
        if (_soundData == null) { return; }

        string goodPath = Path.Combine(Application.persistentDataPath, _saveFileName);
        string legacyPath = Application.persistentDataPath + _saveFileName;

        string pathToUse = null;
        if (File.Exists(goodPath)) { pathToUse = goodPath; }
        else if (File.Exists(legacyPath)) { pathToUse = legacyPath; }

        if (string.IsNullOrEmpty(pathToUse)) { return; }

        string json = File.ReadAllText(pathToUse);
        JsonUtility.FromJsonOverwrite(json, _soundData);
        Debug.Log("[MonsterSfxBinder] Loaded SfxVolume=" + _soundData.SfxVolume.ToString("F2") + " from " + pathToUse);
    }

    private IEnumerator Loop()
    {
        WaitForSeconds wait = new WaitForSeconds(_rescanIntervalSeconds);
        while (true)
        {
            RefreshTrackedSources();
            ApplyVolumeNow();
            yield return wait;
        }
    }

    private void ApplyVolumeNow()
    {
        float current = Mathf.Clamp01(_soundData != null ? _soundData.SfxVolume : 1f);

        bool changedValue = !Mathf.Approximately(current, _lastApplied);
        bool changedCount = (_tracked.Count != _lastAppliedTrackedCount);

        if (!changedValue && !changedCount)
        {
            return;
        }

        foreach (AudioSource s in _tracked)
        {
            if (s == null) { continue; }

            float baseVol;
            if (!_baseVolume.TryGetValue(s, out baseVol))
            {
                baseVol = s.volume;           
                _baseVolume[s] = baseVol;
            }

            s.volume = baseVol * current;    
        }

        _lastApplied = current;
        _lastAppliedTrackedCount = _tracked.Count;
    }

    private void RefreshTrackedSources()
    {
        for (int t = 0; t < _monsterTags.Length; t++)
        {
            string tag = _monsterTags[t];
            if (string.IsNullOrEmpty(tag)) { continue; }

            GameObject[] roots;
            try
            {
                roots = GameObject.FindGameObjectsWithTag(tag);
            }
            catch
            {
                continue; 
            }

            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i] == null) { continue; }

                AudioSource[] sources = roots[i].GetComponentsInChildren<AudioSource>(true);
                for (int j = 0; j < sources.Length; j++)
                {
                    AudioSource s = sources[j];
                    if (s == null) { continue; }
                    if (_tracked.Contains(s)) { continue; }

                    _tracked.Add(s);
                    if (!_baseVolume.ContainsKey(s))
                    {
                        _baseVolume.Add(s, s.volume);
                    }
                }
            }
        }

        List<AudioSource> dead = null;
        foreach (AudioSource s in _tracked)
        {
            if (s == null)
            {
                if (dead == null) { dead = new List<AudioSource>(); }
                dead.Add(s);
            }
        }
        if (dead != null)
        {
            for (int i = 0; i < dead.Count; i++)
            {
                _tracked.Remove(dead[i]);
                _baseVolume.Remove(dead[i]);
            }
        }
    }
}
