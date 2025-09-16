using TMPro;
using UnityEngine;

public class BuyOrUpgradeButton : BaseButton
{
    private bool _isGetUnit = false;
    private InventoryData _showUnit;
    private TextMeshProUGUI _text;
    protected override void OnClick()
    {
        base.OnClick();
        if(_isGetUnit)
        {
            UpgradeUnit();
        }
        else
        {
            BuyUnit();
        }
    }
    private void BuyUnit()
    {
        if (DataManager.Instance.GameData.PlayerMoney < _showUnit.UnlockPrice)
        {
            UIManager.Instance.OpenPopUp(PopUpStatus.NoMoneyAlret);
            return;
        }
        DataManager.Instance.GameData.PlayerMoney -= _showUnit.UnlockPrice;
        EventManager.Instance.Invoke("UpdateMoneyText");
        EventManager.Instance.Invoke("BuyUnit");

    }
    private void UpgradeUnit()
    {
        //강화 패널로 넘어가기
        UIManager.Instance.AddPanelStack(PanelStatus.UpgradePanel);
        print("강화");
    }

    private void Update()
    {
        _showUnit = UnitManager.Instance.ChoiceUnit;
        _isGetUnit = UnitManager.Instance.IsGetUnit(_showUnit.UnitType);
        ChangeText();
    }
    private void ChangeText()
    {
        if (_isGetUnit)
        {
            _buttonText.text = "강화";
        }
        else
        {
            _buttonText.text = "구매"; 
        }
    }

}
