using UnityEngine;

public class NoChoiceUnitAlret : PopUp
{
    protected override void SetStatus()
    {
        _status = PopUpStatus.NoChoiceUnitAlret;
    }
}
