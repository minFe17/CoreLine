using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class TowerUnit : Unit
{
    [SerializeField] protected EUnitType _unitType;
    [SerializeField] List<GameObject> _levelUnit;

    UnitLevelData _data;

    int _originalLayer;

    public event Action OnUpgrade;
    public EUnitType UnitType { get => _unitType; }

    bool IsMaxLevel() => _level >= _levelUnit.Count - 1;

    void OnEnable()
    {
        _level = 0;
        UpgradeCharacter();
        SetLevel();
        _unitStateData = _data.UnitState;
        _currentHp = _unitStateData.HP;
        _isDie = false;
    }

    // Test
    void Update()
    {
        LookTarget();
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (_level == _levelUnit.Count - 1)
                return;
            Upgrade();
        }
        if(Input.GetKeyDown(KeyCode.Alpha4))
            TakeDamage(30);
    }

    public override void ClickUnit()
    {
        base.ClickUnit();
        SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorType.OpenUnitUI, _cell);
        if (_level != _levelUnit.Count - 1)
            return;
        Fusion();
    }

    public override void Die()
    {
        base.Die();
        MonoSingleton<ObjectPoolManager>.Instance.Push(_unitType, gameObject);
    }

    void SetLevel()
    {
        for (int i = 0; i < _levelUnit.Count; i++)
        {
            if (i == _level)
                _levelUnit[i].SetActive(true);
            else
                _levelUnit[i].SetActive(false);
        }
    }

    void UpgradeCharacter()
    {
        _data = SimpleSingleton<UnitDataList>.Instance.GetUnitData(_unitType).LevelData[_level];
        _unitStateData = _data.UnitState;
        _animator = _levelUnit[_level].GetComponent<Animator>();

        DieEvent dieEvent;
        if (GetCurrentUnit().TryGetComponent<DieEvent>(out dieEvent))
            return;
        dieEvent = GetCurrentUnit().AddComponent<DieEvent>();
        dieEvent.Init(this);
    }

    void SetLayerRecursively(GameObject targetObject, int layer)
    {
        targetObject.layer = layer;
        foreach (Transform child in targetObject.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }

    public GameObject GetCurrentUnit()
    {
        return _levelUnit[_level];
    }

    public void SetFusionLayer(string layerName)
    {
        _originalLayer = gameObject.layer;

        int fusionLayer = LayerMask.NameToLayer(layerName);
        SetLayerRecursively(_levelUnit[_level], fusionLayer);
    }

    public void RestoreOriginalLayer()
    {
        SetLayerRecursively(_levelUnit[_level], _originalLayer);
    }

    public void Upgrade()
    {
        if (IsMaxLevel())
            return;

        _level++;
        UpgradeCharacter();
        SetLevel();
        OnUpgrade?.Invoke();
        
        if (IsMaxLevel())
            SimpleSingleton<FusionManager>.Instance.AddFusionableUnit(_unitType, this);
    }

    public void Fusion()
    {
        SimpleSingleton<FusionManager>.Instance.Fusion(this);
    }
}