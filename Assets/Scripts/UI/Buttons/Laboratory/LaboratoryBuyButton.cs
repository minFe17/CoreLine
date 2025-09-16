using System.Collections.Generic;
using UnityEngine;

public class LaboratoryBuyButton : BaseButton
{
    protected override void OnClick()
    {
        base.OnClick();
        if (DataManager.Instance.GameData.PlayerGem < LaboratoryManager.Instance.ChoiceLaboratory.Cost)
        {
            UIManager.Instance.OpenPopUp(PopUpStatus.NoMoneyAlret);
            return;
        }

        DataManager.Instance.GameData.PlayerGem -= LaboratoryManager.Instance.ChoiceLaboratory.Cost;
        EventManager.Instance.Invoke("UpdateMoneyText");
        EventManager.Instance.Invoke<LaboratoryData>("BuyLaboratory",LaboratoryManager.Instance.ChoiceLaboratory);
        EventManager.Instance.Invoke("UpdateLaboratoryInfo");
    }

}
