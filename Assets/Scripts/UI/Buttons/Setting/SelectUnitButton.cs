using UnityEngine;

public class SelectUnitButton : UnitButton
{

    protected override void OnClick()
    {
        EventManager.Instance.Invoke<EUnitType>("ChangeUnit", _data.UnitType);
        EventManager.Instance.Invoke("SelectUpdate");
    }

}
