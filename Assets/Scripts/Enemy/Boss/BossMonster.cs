using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]
public class BossMonster : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Collider2D _col;
    private HitFlashTest _hitFlash;

    [SerializeField] private int _maxHp = 500;
    [SerializeField] private float _dieDeactivateDelay = 2.0f;
    private int _currentHp = 0;
    private bool _isDead = false;
    public bool IsDead => _isDead;

    [SerializeField] private string _moveBool = "isMoving";
    [SerializeField] private string _attackTrigger = "Attack";
    [SerializeField] private string _dieTrigger = "Die";
    [SerializeField] private string _attackStateName = "Attack";

    
    private bool _attackLocked = false;

    private GameObject _hpBar;
    private Image _hpBarImage;
    [SerializeField] private string _hpBarPrefabPath = "Monster/Prefab/MonsterHpBar";
    [SerializeField] private Vector2 _hpBarOffset = new Vector2(0f, 1.4f);

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
        _hitFlash = GetComponent<HitFlashTest>();
    }

    private void OnEnable()
    {
        _isDead = false;
        _attackLocked = false;
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
        if (_animator) _animator.SetBool(_moveBool, false);

        if (_attackLocked && _animator && !_animator.IsInTransition(0))
        {
            AnimatorStateInfo st = _animator.GetCurrentAnimatorStateInfo(0);
            if (!st.IsName(_attackStateName)) _attackLocked = false;
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
    }

    public void FireDieTrigger() => _animator.SetTrigger(_dieTrigger);

    public bool IsAttackReady()
    {
        return !_attackLocked && !_isDead;
    }

    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        _currentHp -= Mathf.Max(0, damage);

        if (_hitFlash != null)
            _hitFlash.TriggerHitEffect();

        if (_currentHp <= 0)
            StartCoroutine(DieSequence());
    }

    private IEnumerator DieSequence()
    {
        if (_isDead) yield break;
        _isDead = true;

        if (_col) _col.enabled = false;

        _attackLocked = false;
        _animator.ResetTrigger(_attackTrigger);
        _animator.SetTrigger(_dieTrigger);

        if (_hitFlash != null)
            _hitFlash.ClearFlash();

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
