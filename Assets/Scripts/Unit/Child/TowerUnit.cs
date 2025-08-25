using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class TowerUnit : Unit
{

    [SerializeField] protected EUnitType _unitType;
    [SerializeField] List<GameObject> _levelUnit;

    int _originalLayer;

    public event Action OnUpgrade;
    public EUnitType UnitType { get => _unitType; }

    bool IsMaxLevel() => _level >= _levelUnit.Count - 1;

    void OnEnable()
    {
        _level = 0;
        UpgradeCharacter();
        SetLevel();
    }

    // Test
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (_level == _levelUnit.Count - 1)
                return;
            Upgrade();
        }
    }

    private void OnMouseUp()
    {
        if (_level != _levelUnit.Count - 1)
            return;
        Fusion();
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
        _animator = _levelUnit[_level].GetComponent<Animator>();
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
        // 퓨전 가능한 유닛 누르면 지금 유닛이랑 그 유닛 없애고 이 위치에 퓨전 유닛 소환
        SimpleSingleton<FusionManager>.Instance.Fusion(this);
    }
}