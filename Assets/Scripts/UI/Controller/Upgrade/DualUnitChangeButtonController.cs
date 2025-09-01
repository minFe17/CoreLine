using UnityEngine;

public class DualUnitChangeButtonController : DualButtonController<UnlockedUnit>
{
    public override void OnClickNextButton()
    {
        base.OnClickNextButton();
        EventManager.Instance.Invoke<EUnitType>("ChangeChoiceUnitData", _list[_index].UnitType);
    }
    public override void OnClickPrevButton()
    {
        base.OnClickPrevButton();
        EventManager.Instance.Invoke<EUnitType>("ChangeChoiceUnitData", _list[_index].UnitType);
    }
    protected override void SettingList()
    {
        _list = UnitManager.Instance.UnlockedUnits;
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.Instance.Invoke<EUnitType>("ChangeChoiceUnitData", _list[_index].UnitType);
    }
    protected override void SettingIndex()
    {
        for (int i = 0; i < _list.Count; i++)
        {
            if (_list[i].UnitType == UnitManager.Instance.ChoiceUnit.UnitType)
            {
                _index = i;
                return;
            }
        }
    }
}
