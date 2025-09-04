using System.Collections.Generic;
using UnityEngine;

public class LaboratoryBuyButton : BaseButton
{
    protected override void OnClick()
    {
        //돈있나없나 체크하고깎기
        EventManager.Instance.Invoke<LaboratoryData>("BuyLaboratory",LaboratoryManager.Instance.ChoiceLaboratory);

    }

}
