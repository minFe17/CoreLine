using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public class TowerUnit : Unit
{
    [SerializeField] protected EUnitType _unitType;
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
        {
            if (_level == _levelUnit.Count - 1)
                Fusion();
            Upgrade();
        }
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

    public void Upgrade()
    {
        if (_level >= _levelUnit.Count - 1)
            return;
        _level++;
        UpgradeCharacter();
        SetLevel();
        OnUpgrade?.Invoke();
    }

    public void Fusion()
    {
        // 퓨전 가능한 유닛이 있는 타일만 밝게?
        // 그 유닛 누르면 지금 유닛이랑 그 유닛 없애고 이 위치에 퓨전 유닛 소환
    }
}