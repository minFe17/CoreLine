using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class TowerUnit : Unit, ILevelUp, IFusion
{
    [SerializeField] List<GameObject> _levelUnit;

    public event Action OnUpgrade;

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
            Upgrade();
    }

    void OnMouseDown()
    {
        // UI º“»Ø
        SimpleSingleton<AttackRangeManager>.Instance.CheckAttackRange(this);
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

    public GameObject GetCurrentUnit()
    {
        return _levelUnit[_level];
    }

    #region Interface
    public void Upgrade()
    {
        _level++;
        UpgradeCharacter();
        SetLevel();
        OnUpgrade?.Invoke();
    }

    void IFusion.Fusion()
    {

    }
    #endregion
}