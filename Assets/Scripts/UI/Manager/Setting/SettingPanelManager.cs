using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SettingPanelManager : MonoBehaviour
{
    private PoolingManager _selectableUnitButtons;
    private PoolingManager _selectedUnitButtons;
    private List<UnlockedUnit> _selectableUnitDatas;
    private List<UnlockedUnit> _selectedUnitDatas = new();

    private GameObject _content;
    private GameObject _seleced;


    public void OnClickPlayButton()
    {
        if(_selectedUnitDatas.Count <= 0)
        {
            UIManager.Instance.OpenPopUp(PopUpStatus.NoChoiceUnitAlret);
            return;
        }
        UIManager.Instance.AddPanelStack(PanelStatus.PlayPanel);
    }
    public void OnClickUpgradeButton()
    {
        UIManager.Instance.AddPanelStack(PanelStatus.UpgradePanel);
    }

    private void Start()
    {
        _content = GameObject.Find("Content");
        _seleced = GameObject.Find("SelectedUnit");
        CreateSelectableButtons();
        CreateSelectedButtons();

    }
    private void OnEnable()
    {
        UpdateSelectableButtons();

        EventManager.Instance.Subscribe("AddSelectedUnit", AddSelected);
        EventManager.Instance.Subscribe("DeleteSelectedUnit", RemoveSelected);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("AddSelectedUnit", (Action)AddSelected);
        EventManager.Instance.UnSubscribe("DeleteSelectedUnit", (Action)RemoveSelected);
    }
    private void CreateSelectableButtons()
    {
        _selectableUnitDatas = UnitManager.Instance.UnlockedUnits;
        _selectableUnitButtons = new PoolingManager("UI/Prefabs/Button/Setting/SelectableUnitButton", _content, UnitManager.Instance.AllUnitCount());
        for (int i = 0; i < _selectableUnitDatas.Count; i++)
        {
            if (_selectableUnitDatas[i].UnitType == EUnitType.King) continue;
            SelectUnitButton btn = _selectableUnitButtons.Pop().GetComponent<SelectUnitButton>();
            btn.Data = UnitManager.Instance.GetInventoryData(_selectableUnitDatas[i].UnitType);
            print(btn.Data.UnitType);
        }
    }
    private void CreateSelectedButtons()
    {
        foreach(var unit in UnitManager.Instance.SettingUnits)
        {
            _selectedUnitDatas.Add(unit.Value);
        }
        _selectedUnitButtons = new PoolingManager("UI/Prefabs/Button/Setting/SelectedUnitButton", _seleced, 10);
        foreach (var key in _selectedUnitDatas)
        {
            SelectUnitButton btn = _selectedUnitButtons.Pop().GetComponent<SelectUnitButton>();
            btn.Data = UnitManager.Instance.GetInventoryData(key.UnitType);
        }
    }
    private void UpdateSelectableButtons()
    {
        if (_selectableUnitButtons == null) return;
        foreach (GameObject obj in _selectableUnitButtons.GetAllToActiveTrue())
        {
            SelectUnitButton btn = obj.GetComponent<SelectUnitButton>();
            btn.gameObject.SetActive(false);
        }
        for (int i = 0; i < _selectableUnitDatas.Count; i++)
        {
            if (_selectableUnitDatas[i].UnitType == EUnitType.King) continue;
            SelectUnitButton btn = _selectableUnitButtons.Pop().GetComponent<SelectUnitButton>();
            btn.Data = UnitManager.Instance.GetInventoryData(_selectableUnitDatas[i].UnitType);
        }
    }
    private void UpdateSelectedButtons() 
    {
        foreach (GameObject obj in _selectedUnitButtons.GetAllToActiveTrue())
        {
            SelectUnitButton btn = obj.GetComponent<SelectUnitButton>();
            btn.gameObject.SetActive(false);
        }//전부 꺼버리고 리스트대로 다시 켜주기

        for(int i=0;i< _selectedUnitDatas.Count;i++)
        {
            SelectUnitButton btn = _selectedUnitButtons.Pop().GetComponent<SelectUnitButton>();
            btn.Data = UnitManager.Instance.GetInventoryData(_selectedUnitDatas[i].UnitType);
            btn.transform.SetSiblingIndex(i);
        }

    }
    private void AddSelected()
    {
        _selectedUnitDatas.Add(UnitManager.Instance.GetUnlockedUnit(UnitManager.Instance.ChoiceUnit.UnitType));
        UpdateSelectedButtons();
    }
    private void RemoveSelected()
    {
        for (int i = 0; i < _selectedUnitDatas.Count; i++)
        {
            if (_selectedUnitDatas[i].UnitType == UnitManager.Instance.ChoiceUnit.UnitType)
            {
                _selectedUnitDatas.RemoveAt(i);
            }
        }
        UpdateSelectedButtons();
    }
}
