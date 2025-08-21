using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(MonsterMover))]
public class Monster : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private MonsterMover _mover;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _mover = GetComponent<MonsterMover>();
    }

    void Update()
    {
        bool isMoving = _mover != null && _mover.IsFollowingPath;
        _animator.SetBool("isMoving", isMoving);
    }

    public void SetFlip(Vector3 delta)
    {
        if (Mathf.Abs(delta.x) > 0.0001f)
            _spriteRenderer.flipX = (delta.x < 0f);
    }
}
