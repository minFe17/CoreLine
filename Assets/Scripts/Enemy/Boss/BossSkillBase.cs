using System.Collections;
using UnityEngine;


public abstract class BossSkillBase : MonoBehaviour
{
    [Header("Skill Common")]
    [SerializeField] private float _weight = 1f;
    [SerializeField] private float _cooldown = 6f;
    [SerializeField] private float _castTime = 0.6f;
    [SerializeField] private float _postDelay = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioClip _sfx;
    [Range(0f, 1f)]
    [SerializeField] private float _sfxVolume = 1f;
    protected AudioSource _audio;

    private bool _lastCastSucceeded = false;
    public bool LastCastSucceeded { get { return _lastCastSucceeded; } }
    public void ResetCastOutcome() { _lastCastSucceeded = false; }
    protected void MarkCastSuccess() { _lastCastSucceeded = true; }

    protected BossController _controller;
    protected BossMonster _boss;
    protected TestMap _map;
    protected MonsterManager _monsterManager;



    protected float _readyAt = 0f;

    public float Weight => _weight;
    public float Cooldown => _cooldown;
    public float CastTime => _castTime;
    public float PostDelay => _postDelay;

    public virtual void Setup(BossController controller)
    {
        _controller = controller;
        _boss = controller ? controller.Boss : null;
        _map = controller ? controller.Map : null;
        _monsterManager = controller ? controller.MonsterManager : null;

        _audio = GetComponent<AudioSource>();
        if (_audio == null)
            _audio = gameObject.AddComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.spatialBlend = 0f;
    }

    public virtual bool CanCast(BossController controller)
    {
        return Time.time >= _readyAt;
    }


    public IEnumerator Execute(BossController controller)
    {
        
        ResetCastOutcome();

        if (!CanCast(controller))
        {
            yield break; 
        }

        if (_castTime > 0f)
        {
            yield return new WaitForSeconds(_castTime);
        }

        // 루틴중 사운드 재생
        PlaySfx();

        Perform(controller);

        if (!LastCastSucceeded)
        {
            yield break;
        }

        if (_postDelay > 0f)
        {
            yield return new WaitForSeconds(_postDelay);
        }
        ArmCooldown();
    }


    protected void ArmCooldown()
    {
        _readyAt = Time.time + _cooldown;
    }

    protected void PlaySfx()
    {
        if (_sfx != null && _audio != null)
            _audio.PlayOneShot(_sfx, _sfxVolume);
    }

    protected abstract void Perform(BossController controller);
}