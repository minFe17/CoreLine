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
        //�����Ͻðڽ��ϱ�? ���� �����ϸ� �����. �������� �ܾ׺��� ���� 
        //�̷��� ���������λ����ɰͰ�����
        print("����");
    }
    private void UpgradeUnit()
    {
        //��ȭ �гη� �Ѿ��
        print("��ȭ");
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
            _buttonText.text = "��ȭ";
        }
        else
        {
            _buttonText.text = "����"; 
        }
    }

}
