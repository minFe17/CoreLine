using TMPro;
using UnityEngine;

public class ShowUnitController: MonoBehaviour
{
    private TextMeshProUGUI _text;
    private Animator _animator;
    private void Awake()
    {
        FindAndGetComponent();
        EventManager.Instance.Subscribe<EUnitType>("ChangeUnit", ChangeAnimation);
        EventManager.Instance.Subscribe<EUnitType>("ChangeUnit", ChangeText);
    }

    private void FindAndGetComponent()
    {
        Transform trans = transform.Find("UnitAnimation");
        _animator = trans.GetComponent<Animator>();
        trans = transform.Find("InformationBox/InfoText");
        if (trans != null)
        {
            _text = trans.GetComponent<TextMeshProUGUI>();
        }
    }
    private void ChangeText(EUnitType param)
    {
        if (_text == null) return;
        InventoryData data = UnitManager.Instance.GetInventoryData(param);
        _text.text = data.Information;
    }
    private void ChangeAnimation(EUnitType param)
    {
        EventManager.Instance.Invoke<EUnitType>("ChangeChoiceUnitData", param);
        _animator.SetInteger("Unit", MatchingUnit(param));
    }
    private int MatchingUnit (EUnitType param)
    {
        switch (param)
        {
            case EUnitType.King:
                return 0;
            case EUnitType.Wizard:
                return 1;
            case EUnitType.Pirate:
                return 2;
            case EUnitType.Warrior:
                return 3;
            case EUnitType.Chef:
                return 4;
            case EUnitType.Archer:
                return 5;

        }
        return 0;
    }
}
