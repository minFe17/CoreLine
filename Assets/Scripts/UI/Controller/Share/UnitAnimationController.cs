using UnityEngine;

public class UnitAnimationController : MonoBehaviour
{
    [SerializeField]
    private EUnitType _unitType;

    private Animator _animator;

    
    public EUnitType UnitType
    {
        get { return _unitType; }
    }
    public void Attack()
    {
        //print(_unitType.ToString());
    }
    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }
    private void OnEnable()
    {
        //_animator.SetTrigger("doAttack"); 이건 좀 더 알아보자
    }
}
