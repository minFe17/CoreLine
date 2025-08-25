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
}
