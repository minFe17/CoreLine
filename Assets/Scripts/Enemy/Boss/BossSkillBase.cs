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
        // 0) 시전 결과 초기화
        ResetCastOutcome();

        // 1) JIT 사전검사: 지금 시전 가능한가? (개별 스킬이 오버라이드할 CanCast에서 환경 체크)
        if (!CanCast(controller))
        {
            yield break; // 불가능 → 대기/쿨타임 없이 즉시 종료
        }

        // 2) 캐스팅 시간
        if (_castTime > 0f)
        {
            yield return new WaitForSeconds(_castTime);
        }

        PlaySfx();

        // 3) 실제 수행(스킬 내부에서 MarkCastSuccess()를 호출해야 '성공'으로 인정됨)
        Perform(controller);

        // 4) 실패면(효과 미발생) 바로 종료: 후딜/쿨 없음
        if (!LastCastSucceeded)
        {
            yield break;
        }

        // 5) 후딜
        if (_postDelay > 0f)
        {
            yield return new WaitForSeconds(_postDelay);
        }

        // 6) 쿨타임 무장(이 베이스 구조에서는 스킬이 자체적으로 쿨을 관리)
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