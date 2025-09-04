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

    private float _dieDeactivateDelay = 1.8f;

    
    private string _moveBool = "isMoving";
    private string _attackTrigger = "Attack";
    private string _dieTrigger = "Die";
    private string _attackStateName = "Attack";
    private bool _attackLocked = false;

 
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
