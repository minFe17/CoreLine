using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(MonsterMover))]
public class Monster : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private MonsterMover _mover;
    private Collider2D _col;
   
    private GameObject _hpBar;
    private Vector2 _hpBarOffset = new Vector2(0f, 1.2f);
    private Image _hpBarImage;
    private HitFlashTest _hitFlash;

    private int _currentHp = 0;
    private int _maxHp = 100;
    private float _dieDeactivateDelay = 1.8f;
    private bool _isDead = false;
    public bool IsDead => _isDead;

    private string _moveBool = "isMoving";
    private string _attackTrigger = "Attack";
    private string _dieTrigger = "Die";

    private string _attackStateName = "Attack";
    private bool _attackLocked = false;


    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _mover = GetComponent<MonsterMover>();
        _col = GetComponent<Collider2D>();
       _hitFlash = GetComponent<HitFlashTest>();
    }

    private void OnEnable()
    {
        _isDead = false;
        _attackLocked = false;
        _currentHp = Mathf.Max(1, _maxHp);

        GameObject hpBarPanel = GameObject.FindGameObjectWithTag("BarPanel");
        GameObject hpBarPrefab = Resources.Load<GameObject>("Monster/Prefab/MonsterHpBar");

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
        HpBarUpdate();

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            TakeDamage(10);
        }


        bool isMoving = _mover != null && _mover.IsFollowingPath;
        _animator.SetBool(_moveBool, isMoving);

        if (_attackLocked)
        {
            if (!_animator.IsInTransition(0))
            {
                AnimatorStateInfo st = _animator.GetCurrentAnimatorStateInfo(0);
                if (!st.IsName(_attackStateName)) _attackLocked = false;
            }
        }
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
        return !_attackLocked;
    }

    public void TakeDamage(int damage)
    {
        if (_isDead)
            return;

        _currentHp -= damage;

        if (_hitFlash != null)
            _hitFlash.TriggerHitEffect();
       
        if (_currentHp <= 0)
        {
           StartCoroutine(DieSequence());
        }
        
    }

    private IEnumerator DieSequence()
    {
        if (_isDead) yield break;
        _isDead = true;

        if (_mover) _mover.OnOwnerDied();
        if (_col) _col.enabled = false;

        _attackLocked = false;                         
        _animator.ResetTrigger(_attackTrigger);
        _animator.SetTrigger(_dieTrigger);

        if(_hitFlash != null)
            _hitFlash.ClearFlash();

        yield return new WaitForSeconds(_dieDeactivateDelay);

        gameObject.SetActive(false);
    }

    protected void HpBarUpdate()
    {
        if (_hpBar == null || _hpBarImage == null)
            return;

        Vector2 pos = transform.position;
        _hpBar.transform.position = Camera.main.WorldToScreenPoint(pos + _hpBarOffset);
        _hpBarImage.fillAmount = (float)_currentHp / (float)_maxHp;
    }
}
