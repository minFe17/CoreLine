using NaughtyAttributes;
using UnityEngine;
using Utils;

public class Unit : MonoBehaviour
{
    [SerializeField] protected EUnitType _unitType;
    [ShowIf("IsNotKing")]
    [SerializeField] protected int _level = 1;

    protected UnitLevelData _data;

    Animator _animator;
    int _currentHp;
    bool _isDie;

    public UnitLevelData Data { get => _data; }
    public bool IsDie { get => _isDie; }

    #region NaughtyAttributes
    bool IsNotKing() => _unitType != EUnitType.King;
    #endregion

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (_data == null)
            _data = SimpleSingleton<UnitDataList>.Instance.GetUnitData(_unitType).LevelData[_level - 1];
        _currentHp = _data.HP;
        _isDie = false;
    }

    public void TakeDamage(int damage)
    {
        if (_isDie)
            return;

        _currentHp -= damage;
        if (_currentHp <= 0)
            _animator.SetTrigger("doDie");
        else
            _animator.SetTrigger("doHit");
    }

    #region Animation Event
    protected virtual void Die()
    {
        _isDie = true;
        // 오브젝트 풀링
    }
    #endregion
}