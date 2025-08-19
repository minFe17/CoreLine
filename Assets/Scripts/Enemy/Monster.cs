//using TMPro;
//using UnityEngine;

//public class Monster : MonoBehaviour
//{
//    private Animator _animator;
//    private Rigidbody2D _rigid;
//    private SpriteRenderer _spriteRenderer;
//    private bool _isClicked = false;

//    [SerializeField] private LayerMask interactMask = ~0;
//    [SerializeField] private Camera clickCamera;
//    private static Camera _cachedCam;
//    string outlineLayer = "Monster";
//    string normalLineLayer = "Default";
//    int _outLayer, _normalLayer;

//    public int Row { get; private set; } = -1;
//    public int Col { get; private set; } = -1;

//    private void Awake()
//    {
//        _animator = GetComponent<Animator>();
//        _rigid = GetComponent<Rigidbody2D>();
//        _spriteRenderer = GetComponent<SpriteRenderer>();

//        if (_cachedCam == null)
//        {
//            _cachedCam = clickCamera != null ? clickCamera : Camera.main;
//        }

//        _outLayer = LayerMask.NameToLayer(outlineLayer);
//        _normalLayer = LayerMask.NameToLayer(normalLineLayer);
//        SetLayer(gameObject, _normalLayer);
//    }

//    private void Update()
//    {
//        //_horizontalInput = Input.GetAxis("Horizontal");
//        //PlayAnimation();


//        if (!Input.GetMouseButtonDown(0)) 
//            return;

//        Ray ray = _cachedCam.ScreenPointToRay(Input.mousePosition);
//        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, interactMask);

//        if (!hit.collider) 
//            return;

//        Monster m = hit.collider.GetComponentInParent<Monster>();

//        if (m != this) 
//            return;

//        ToggleClicked();
//        ToggleLayer(gameObject);
//    }

//    //private void FixedUpdate()
//    //{
//    //    transform.Translate(Vector3.right * _horizontalInput * _moveSpeed * Time.fixedDeltaTime);
//    //    Vector3 pos = transform.position;
//    //    pos.x = Mathf.Clamp(pos.x, _minX, _maxX);
//    //    transform.position = pos;
//    //}

//    private void PlayAnimation()
//    {
//        bool isLeft = Input.GetKey(KeyCode.LeftArrow);
//        bool isRight = Input.GetKey(KeyCode.RightArrow);
//        bool isDown = Input.GetKey(KeyCode.DownArrow);

//        bool isMove = isLeft || isRight;
//        _animator.SetBool("isMoving", isMove);


//        if (Input.GetKeyDown(KeyCode.Alpha1))
//        {
//            _animator.SetTrigger("Attack");
//        }

//        if (Input.GetKeyDown(KeyCode.Alpha2))
//        {
//            _animator.SetTrigger("Die");
//        }

//        if (isLeft)
//            transform.localScale = new Vector3(-1, 1, 1);
//        else if (isRight)
//            transform.localScale = new Vector3(1, 1, 1);
//    }




//    private void ToggleClicked()
//    {
//        _isClicked = !_isClicked;
//        _animator.SetBool("isMoving", _isClicked);
//    }

//    void ToggleLayer(GameObject go)
//    {
//        int target = (go.layer == _outLayer) ? _normalLayer : _outLayer;
//        SetLayer(go, target);
//    }
//    void SetLayer(GameObject go, int layer)
//    {
//        go.layer = layer;
//        for (int i = 0; i < go.transform.childCount; i++)
//            SetLayer(go.transform.GetChild(i).gameObject, layer);
//    }
//}


using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Collider2D))]
public class Monster : MonoBehaviour
{
    private Animator _animator;

    [SerializeField] private LayerMask interactMask = ~0;
    [SerializeField] private Camera clickCamera;
    private static Camera _cachedCam;

    [SerializeField] private string outlineLayer = "Monster"; 
    [SerializeField] private string normalLayer = "Default"; 
    private int _outLayer, _normalLayer;

    [SerializeField] private string dieTriggerName = "Die";
    [SerializeField] private float disableDelay = 0.8f;

    public int Row { get; private set; } = -1;
    public int Col { get; private set; } = -1;

    private bool _isClicked;

    public bool IsMarked { get; private set; } = false;

    public bool IsOutlinedVisual => gameObject.layer == _outLayer;

    public bool IsCountedForBingo => IsMarked || IsOutlinedVisual;

    private void Awake()
    {
        _animator = GetComponent<Animator>();

        if (_cachedCam == null)
            _cachedCam = clickCamera != null ? clickCamera : Camera.main;

        _outLayer = LayerMask.NameToLayer(outlineLayer);
        _normalLayer = LayerMask.NameToLayer(normalLayer);

        SetLayerRecursive(gameObject, _normalLayer);
    }

    public void SetCoords(int r, int c) { Row = r; Col = c; }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        Ray ray = _cachedCam.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, interactMask);
        if (!hit.collider) return;

        var m = hit.collider.GetComponentInParent<Monster>();
        if (m != this) return;

        _isClicked = !_isClicked;
        _animator.SetBool("isMoving", _isClicked);
       
        ToggleOutline();
        BoardManager.Instance?.OnMonsterOutlineChanged(Row, Col);
    }

    public void ToggleOutline()
    {
        int target = (gameObject.layer == _outLayer) ? _normalLayer : _outLayer;
        SetLayerRecursive(gameObject, target);
    }

    public void PlayDeathAndDisable()
    {
        IsMarked = true;

        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        if (!string.IsNullOrEmpty(dieTriggerName))
            _animator.SetTrigger(dieTriggerName);

        Invoke(nameof(DisableSelf), disableDelay);
    }

    private void DisableSelf() => gameObject.SetActive(false);

    private void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        for (int i = 0; i < go.transform.childCount; i++)
            SetLayerRecursive(go.transform.GetChild(i).gameObject, layer);
    }
}
