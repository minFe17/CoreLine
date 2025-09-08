using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class TowerUnit : Unit
{
    [SerializeField] protected EUnitType _unitType;
    [SerializeField] List<GameObject> _levelUnit;

    UnitLevelData _data;

    int _level;
    int _originalLayer;
    int _unitTotalCost;

    public event Action OnUpgrade;

    public EUnitType UnitType { get => _unitType; }
    public int Level { get => _level; }
    public int UnitTotalCost { get => _unitTotalCost; }

    public bool IsMaxLevel() => _level >= _levelUnit.Count - 1;

    void OnEnable()
    {
        _level = 0;
        UpgradeCharacter();
        SetLevel();
        _unitStateData = _data.UnitState;
        _currentHp = _unitStateData.HP;
        _isDie = false;
        _unitTotalCost = _data.Cost;
    }

    void Update()
    {
        LookTarget();
    }

    public override void ClickUnit()
    {
        base.ClickUnit();
        if(SimpleSingleton<FusionManager>.Instance.IsFusionMode)
        {
            Fusion();
            return;
        }
        SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorType.OpenUnitUI, this);
    }

    public override void Die()
    {
        base.Die();
        SimpleSingleton<MapUnitManager>.Instance.AddDieUnit();
    }

    public override void Remove()
    {
        base.Remove();
        MonoSingleton<ObjectPoolManager>.Instance.Push(_unitType, gameObject);
        if (SimpleSingleton<AttackRangeManager>.Instance.IsSameUnit(this))
            SimpleSingleton<AttackRangeManager>.Instance.HideAttackRange();
        if (UnitUI.Unit == this)
            UnitUI.Close();
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
        _unitTotalCost += _data.Cost;
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
        int cost = SimpleSingleton<UnitDataList>.Instance.GetUnitData(_unitType).LevelData[_level+1].Cost;
        if (!CostManager.Instance.TrySpend(CostManager.CostType.Unit, cost))
            return;

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