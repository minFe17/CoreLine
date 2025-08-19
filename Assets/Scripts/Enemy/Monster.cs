using TMPro;
using UnityEngine;

public class Monster : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody2D _rigid;
    private SpriteRenderer _spriteRenderer;
    private MonsterMover _mover;

    public int Row { get; private set; } = -1;
    public int Col { get; private set; } = -1;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigid = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _mover = GetComponent<MonsterMover>();
      
    }

    private void Update()
    {
        PlayAnimation();
    }

    private void PlayAnimation()
    {
        bool isMove = _mover != null && _mover.IsFollowingPath; 
        _animator.SetBool("isMoving", isMove);
    }

    public void SetFlip(Vector3 param)
    {
        if (Mathf.Abs(param.x) > 0.0001f)
            _spriteRenderer.flipX = (param.x < 0f);
    }
}
