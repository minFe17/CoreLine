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
        // ǻ�� ������ ������ �ִ� Ÿ�ϸ� ���?
        // �� ���� ������ ���� �����̶� �� ���� ���ְ� �� ��ġ�� ǻ�� ���� ��ȯ
    }
}