using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class MonsterAudio : MonoBehaviour
{
    [Header("Hit Sound")]
    [SerializeField] private AudioClip _hitClip;

    [Header("Playback")]
    [Range(0f, 1f)]
    [SerializeField] private float _volume = 1f;
    [SerializeField] private float _pitchMin = 0.96f;
    [SerializeField] private float _pitchMax = 1.04f;

    private AudioSource _source;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.spatialBlend = 0f; 
    }

    public void OnDamaged(int damage, bool isCritical)
    {
        if (_hitClip == null) return;

        float originalPitch = _source.pitch;
        _source.pitch = Random.Range(_pitchMin, _pitchMax);

        _source.PlayOneShot(_hitClip, _volume);

        _source.pitch = originalPitch;
    }
}
