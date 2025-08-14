using TMPro;
using UnityEngine;

public class Monster : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody2D _rigid;
    private SpriteRenderer _spriteRenderer;

   

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigid = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        //_horizontalInput = Input.GetAxis("Horizontal");
        PlayAnimation();
    }

    //private void FixedUpdate()
    //{
    //    transform.Translate(Vector3.right * _horizontalInput * _moveSpeed * Time.fixedDeltaTime);
    //    Vector3 pos = transform.position;
    //    pos.x = Mathf.Clamp(pos.x, _minX, _maxX);
    //    transform.position = pos;
    //}

    private void PlayAnimation()
    {
        bool isLeft = Input.GetKey(KeyCode.LeftArrow);
        bool isRight = Input.GetKey(KeyCode.RightArrow);
        bool isDown = Input.GetKey(KeyCode.DownArrow);

        bool isMove = isLeft || isRight;
        _animator.SetBool("isMoving", isMove);


        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _animator.SetTrigger("Attack");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _animator.SetTrigger("Die");
        }

        if (isLeft)
            transform.localScale = new Vector3(-1, 1, 1);
        else if (isRight)
            transform.localScale = new Vector3(1, 1, 1);
    }

}
