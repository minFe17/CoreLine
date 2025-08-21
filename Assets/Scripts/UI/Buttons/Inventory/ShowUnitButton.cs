using TMPro;
using UnityEngine;

public class ShowUnitButton : BaseButton
{
    private bool _isGetUnit = false;
    private InventoryData _showUnit;
    private TextMeshProUGUI _text;
    protected override void OnClick()
    {
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
        //구매하시겠습니까? 띄우고 구매하면 돈깎기. 돈없으면 잔액부족 띄우기 
        //이런건 프리팹으로빼도될것같은데
        print("구매");
    }
    private void UpgradeUnit()
    {
        //강화 패널로 넘어가기
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
