using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(MonsterMover))]
public class Monster : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private MonsterMover _mover;

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
    }

    private void Update()
    {
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

}
