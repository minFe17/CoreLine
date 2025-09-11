using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(AudioSource))] 
public sealed class BossMonster : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _col;
    private HitFlashTest _hitFlash;

    [SerializeField] private int _maxHp = 500;
    [SerializeField] private float _dieDeactivateDelay = 2.0f;
    private int _currentHp = 0;
    private bool _isDead = false;
    public bool IsDead { get { return _isDead; } }

    [SerializeField] private string _moveBool = "isMoving";
    [SerializeField] private string _attackTrigger = "Attack";
    [SerializeField] private string _dieTrigger = "Die";
    [SerializeField] private string _attackStateName = "Attack";

    private bool _attackLocked = false;

    private GameObject _hpBar;
    private Image _hpBarImage;
    [SerializeField] private string _hpBarPrefabPath = "Monster/Prefab/MonsterHpBar";
    [SerializeField] private Vector2 _hpBarOffset = new Vector2(0f, 1.4f);

    [Header("Attack SFX")]
    [SerializeField] private AudioClip _attackSfx;
    [Range(0f, 1f)]
    [SerializeField] private float _attackSfxVolume = 1f;
    [Tooltip("공격 애니메이션의 정규화 시간(0~1)에서 재생")]
    [Range(0f, 1f)]
    [SerializeField] private float _attackSfxNormalizedTime = 0.0f;

    private AudioSource _audio;
    private bool _attackSfxPlayed = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
        _hitFlash = GetComponent<HitFlashTest>();

        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.spatialBlend = 0f; 
    }

    private void OnEnable()
    {
        _isDead = false;
        _attackLocked = false;
        _attackSfxPlayed = false;
        _currentHp = Mathf.Max(1, _maxHp);

        GameObject hpBarPanel = GameObject.FindGameObjectWithTag("BarPanel");
        GameObject hpBarPrefab = Resources.Load<GameObject>(_hpBarPrefabPath);

        if (hpBarPanel != null && hpBarPrefab != null)
        {
            _hpBar = Instantiate(hpBarPrefab, hpBarPanel.transform);
            _hpBarImage = _hpBar.transform.GetChild(0).GetComponent<Image>();
        }
        else
        {
            _hpBar = null;
            _hpBarImage = null;
        }

        if (_hitFlash != null) _hitFlash.ClearFlash();
    }

    private void OnDisable()
    {
        if (_hpBar) { Destroy(_hpBar); _hpBar = null; _hpBarImage = null; }
        if (_hitFlash != null) _hitFlash.ClearFlash();
    }

    private void Update()
    {
        if (_animator != null) _animator.SetBool(_moveBool, false);

        // 공격 상태 추적 + 정규화 시간에 맞춰 SFX 1회 재생
        if (_animator != null && !_animator.IsInTransition(0))
        {
            AnimatorStateInfo st = _animator.GetCurrentAnimatorStateInfo(0);

            if (st.IsName(_attackStateName))
            {
                if (!_attackSfxPlayed && _attackSfx != null && _audio != null)
                {
                    if (st.normalizedTime >= _attackSfxNormalizedTime)
                    {
                        _audio.PlayOneShot(_attackSfx, _attackSfxVolume);
                        _attackSfxPlayed = true;
                    }
                }
            }
            else
            {
                // 공격 상태가 끝나면 다음 공격을 위해 플래그 리셋
                _attackSfxPlayed = false;
                if (!st.IsName(_attackStateName)) _attackLocked = false;
            }
        }

        HpBarUpdate();
    }

    public void SetFlip(Vector3 delta)
    {
        if (Mathf.Abs(delta.x) > 0.0001f)
            _spriteRenderer.flipX = (delta.x < 0f);
    }

    public void FireAttackTrigger()
    {
        if (_attackLocked) return;
        if (_animator.IsInTransition(0)) return;

        _animator.ResetTrigger(_attackTrigger);
        _animator.SetTrigger(_attackTrigger);

        _attackLocked = true;
        _attackSfxPlayed = false; // 새로운 공격 시작 시 다시 재생 가능 상태로
    }

    public void FireDieTrigger()
    {
        _animator.SetTrigger(_dieTrigger);
    }

    public bool IsAttackReady()
    {
        return !_attackLocked && !_isDead;
    }

    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        _currentHp -= Mathf.Max(0, damage);

        if (_hitFlash != null) _hitFlash.TriggerHitEffect();

        if (_currentHp <= 0) StartCoroutine(DieSequence());
    }

    private IEnumerator DieSequence()
    {
        if (_isDead) yield break;
        _isDead = true;

        if (_col != null) _col.enabled = false;

        _attackLocked = false;
        _animator.ResetTrigger(_attackTrigger);
        _animator.SetTrigger(_dieTrigger);

        if (_hitFlash != null) _hitFlash.ClearFlash();

        yield return new WaitForSeconds(_dieDeactivateDelay);
        gameObject.SetActive(false);
    }

    private void HpBarUpdate()
    {
        if (_hpBar == null || _hpBarImage == null) return;
        Vector2 pos = transform.position;
        _hpBar.transform.position = Camera.main.WorldToScreenPoint(pos + _hpBarOffset);
        _hpBarImage.fillAmount = (float)_currentHp / Mathf.Max(1f, _maxHp);
    }

}
