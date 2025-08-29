using UnityEngine;

public class UnitAnimationController : MonoBehaviour
{
    [SerializeField]
    private EUnitType _unitType;

    private Animator _animator;

    
    public EUnitType UnitType
    {
        get { return _unitType; }
        set { _unitType = value; }
    }
    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
    }
}
