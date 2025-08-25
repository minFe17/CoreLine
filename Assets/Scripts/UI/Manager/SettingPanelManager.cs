using System.Collections.Generic;
using UnityEngine;

public class SettingPanelManager : MonoBehaviour
{
    private PoolingManager _selectableUnitButtons;
    private PoolingManager _selectedUnitButtons;
    private List<UnlockedUnit> _selectableUnitDatas;
    private Dictionary<EUnitType,UnlockedUnit> _selectedUnitDatas;

    private GameObject _content;
    private GameObject _seleced;
    private void Start()
    {
        _content = GameObject.Find("Content");
        _seleced = GameObject.Find("SelectedUnit");
        CreateSelectableButtons();
        CreateSelectedButtons();
        EventManager.Instance.Subscribe("AddSelectedUnit", UpdateSelectedButtons);
        EventManager.Instance.Subscribe("DeleteSelectedUnit", UpdateSelectedButtons);
    }
    private void OnEnable()
    {
        UpdateSelectableButtons();
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
        _selectedUnitDatas = UnitManager.Instance.SettingUnits;
        _selectedUnitButtons = new PoolingManager("UI/Prefabs/Button/Setting/SelectedUnitButton", _seleced, 10);
        foreach (var key in _selectedUnitDatas)
        {
            SelectUnitButton btn = _selectedUnitButtons.Pop().GetComponent<SelectUnitButton>();
            btn.Data = UnitManager.Instance.GetInventoryData(key.Key);
        }
    }
    private void UpdateSelectableButtons()
    {
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
    private void UpdateSelectedButtons() //이거 정렬해주자 (레벨별로? 아니면 선택가능하게?)
    {
        //아니면 해제하는애 빼주고, 추가해주고 이벤트로 등록해버릴까??
        foreach (GameObject obj in _selectedUnitButtons.GetAllToActiveTrue())
        {
            SelectUnitButton btn = obj.GetComponent<SelectUnitButton>();
            btn.gameObject.SetActive(false);
        }
        foreach (var key in _selectedUnitDatas)
        {
            SelectUnitButton btn = _selectedUnitButtons.Pop().GetComponent<SelectUnitButton>();
            btn.Data = UnitManager.Instance.GetInventoryData(key.Key);
        }
    }
}
