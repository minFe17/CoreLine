using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(MonsterMover))]
public class Monster : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private MonsterMover _mover;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _mover = GetComponent<MonsterMover>();
    }

    private void Update()
    {
        // 이동 중 여부를 MonsterMover에서 가져와 애니메이션 제어
        bool isMoving = _mover != null && _mover.IsFollowingPath;
        _animator.SetBool("isMoving", isMoving);
    }

    
    public void SetFlip(Vector3 delta)
    {
        // x 변화가 눈에 띌 때만
        if (Mathf.Abs(delta.x) > 0.0001f)
            _spriteRenderer.flipX = (delta.x < 0f);
    }
}
