using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(MonsterMover))]
[RequireComponent(typeof(HealthComponent))]
public sealed class Monster : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private MonsterMover _mover;
    private Collider2D _col;
    private HitFlashTest _hitFlash;
    private HealthComponent _health;
    private ThorEffect _thorEffect;

    private float _dieDeactivateDelay = 1.8f;
    private float _thorTimer = 0f;
    
    private string _moveBool = "isMoving";
    private string _attackTrigger = "Attack";
    private string _dieTrigger = "Die";
    private string _attackStateName = "Attack";
    private bool _attackLocked = false;
    private bool _inThorEffect = false;



    private bool _deathStarted = false;

    public bool IsDead
    {
        get
        {
            if (_health != null && _health.IsDead) { return true; }
            return _deathStarted;
        }
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _mover = GetComponent<MonsterMover>();
        _col = GetComponent<Collider2D>();
        _hitFlash = GetComponent<HitFlashTest>();
        _health = GetComponent<HealthComponent>();
    }

    private void OnEnable()
    {
        _deathStarted = false;
        _attackLocked = false;
        if (_col != null) { _col.enabled = true; }
        if (_hitFlash != null) { _hitFlash.ClearFlash(); }

        if (_health != null)
        {
            _health.OnDied += OnDiedByHealth;
        }
    }

    private void OnDisable()
    {
        if (_hitFlash != null) { _hitFlash.ClearFlash(); }

        if (_health != null)
        {
            _health.OnDied -= OnDiedByHealth;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // 스킬로 이동속도 +2, 3초간
            _mover.ApplySpeedModifier(MonsterMover.SpeedType.Skill, +2f, 3f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // 스킬로 이동속도 -2, 3초간
            _mover.ApplySpeedModifier(MonsterMover.SpeedType.Skill, -2f, 3f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            // 타일로 이동속도 -1, 5초간
            _mover.ApplySpeedModifier(MonsterMover.SpeedType.Tile, -1f, 5f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            // 보스로 이동속도 +3, 4초간
            _mover.ApplySpeedModifier(MonsterMover.SpeedType.Boss, +3f, 4f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            // 전체 속도 버프/디버프 제거
            _mover.ClearAllSpeedBuff();
        }

        bool isMoving = _mover != null && _mover.IsFollowingPath;
        if (_animator != null)
        {
            _animator.SetBool(_moveBool, isMoving);

            if (_attackLocked && !_animator.IsInTransition(0))
            {
                AnimatorStateInfo st = _animator.GetCurrentAnimatorStateInfo(0);
                if (!st.IsName(_attackStateName)) { _attackLocked = false; }
            }
        }

        if (_animator != null && _mover != null)
        {
            bool inAttack = false;
            if (!_animator.IsInTransition(0))
            {
                AnimatorStateInfo stInfo = _animator.GetCurrentAnimatorStateInfo(0);
                if (stInfo.IsName(_attackStateName))
                {
                    inAttack = true;
                }
            }

            if (inAttack || !isMoving)
            {
                _animator.speed = 1.0f; 
            }
            else
            {
                float baseSpd = Mathf.Max(0.0001f, _mover.BaseMoveSpeed);
                float curSpd = Mathf.Max(0.0001f, _mover.CurrentMoveSpeed);
                float ratio = curSpd / baseSpd;

                float clamped = Mathf.Clamp(ratio, 0.5f, 2.0f);
                _animator.speed = clamped;
            }
        }
        CheckThorEffect();
    }

    public void SetFlip(Vector3 delta)
    {
        if (Mathf.Abs(delta.x) > 0.0001f)
        {
            _spriteRenderer.flipX = (delta.x < 0.0f);
        }
    }

    public void FireAttackTrigger()
    {
        if (_attackLocked) { return; }
        if (_animator != null && _animator.IsInTransition(0)) { return; }

        if (_animator != null)
        {
            _animator.ResetTrigger(_attackTrigger);
            _animator.SetTrigger(_attackTrigger);
        }

        _attackLocked = true;
    }

    public void FireDieTrigger()
    {
        if (_animator != null)
        {
            _animator.SetTrigger(_dieTrigger);
        }
    }

    public bool IsAttackReady()
    {
        return !_attackLocked;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) { return; }

        if (_hitFlash != null)
        {
            _hitFlash.TriggerHitEffect();
        }

        if (_health != null)
        {
            _health.Damage(damage);
        }
    }

    private void OnDiedByHealth()
    {
        if (_deathStarted) { return; }
        StartCoroutine(DieSequence());
    }

    private void CheckThorEffect()
    {
        if (_inThorEffect && _thorEffect != null)
        {
            _thorTimer += Time.deltaTime;

            if (_thorTimer >= _thorEffect.DamageInterval)
            {
                TakeDamage(_thorEffect.Damage);
                _thorTimer = 0f;
            }
        }
    }

    public void EnterThorEffect(ThorEffect effect)
    {
        _thorEffect = effect;
        _inThorEffect = true;
        _thorTimer = 0f;
        TakeDamage(effect.Damage); 
    }

    public void ExitThorEffect()
    {
        _inThorEffect = false;
        _thorEffect = null;
    }

    private IEnumerator DieSequence()
    {
        if (_deathStarted) { yield break; }
        _deathStarted = true;

        if (_mover != null) { _mover.OnOwnerDied(); }
        if (_col != null) { _col.enabled = false; }

        _attackLocked = false;
        if (_animator != null)
        {
            _animator.ResetTrigger(_attackTrigger);
            _animator.SetTrigger(_dieTrigger);
        }

        if (_hitFlash != null)
        {
            _hitFlash.ClearFlash();
        }

        yield return new WaitForSeconds(_dieDeactivateDelay);

        gameObject.SetActive(false);
    }
}
