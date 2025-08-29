using UnityEngine;
using Utils;

public class FusionUnit : Unit
{
    [SerializeField] EFusionUnitType _unitType;

    void OnEnable()
    {
        if(_animator == null)
            _animator = GetComponent<Animator>();
        _unitStateData = SimpleSingleton<FusionDataList>.Instance.GetFusionData(_unitType).FusionUnitState;
        _currentHp = _unitStateData.HP;
    }

    void Update()
    {
        LookTarget();
        if(Input.GetKeyDown(KeyCode.Alpha4))
            TakeDamage(30);
    }

    public override void Die()
    {
        base.Die();
        MonoSingleton<ObjectPoolManager>.Instance.Push(_unitType, gameObject);
    }
}
