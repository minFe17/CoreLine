using UnityEngine;

public class DualStageChangeController : DualButtonController<WorldStageData>
{
    protected override void SettingList()
    {
        //스테이지 리스트 추가하기
        //월드정보만 뽑아서쓰기!
    }
    //public override void OnClickNextButton()
    //{
    //    base.OnClickNextButton();
    //    EventManager.Instance.Invoke<EUnitType>("ChangeChoiceUnitData", _list[_index].UnitType);
    //}
    //public override void OnClickPrevButton()
    //{
    //    base.OnClickPrevButton();
    //    EventManager.Instance.Invoke<EUnitType>("ChangeChoiceUnitData", _list[_index].UnitType);
    //}
    //protected override void SettingList()
    //{
    //    _list = UnitManager.Instance.UnlockedUnits;
    //}
    //protected override void OnEnable()
    //{
    //    base.OnEnable();
    //    EventManager.Instance.Invoke<EUnitType>("ChangeChoiceUnitData", _list[_index].UnitType);
    //}
    //protected override void SettingIndex()
    //{
    //    for (int i = 0; i < _list.Count; i++)
    //    {
    //        if (_list[i].UnitType == UnitManager.Instance.ChoiceUnit.UnitType)
    //        {
    //            _index = i;
    //            return;
    //        }
    //    }
    //}
}
