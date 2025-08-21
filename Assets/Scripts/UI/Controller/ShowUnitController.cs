using TMPro;
using UnityEngine;

public class ShowUnitController: MonoBehaviour
{
    private TextMeshProUGUI _text;
    private Animator _animator;
    private void Awake()
    {
        FindAndGetComponent();
        EventManager.Instance.Subscribe<string>("ChangeUnit", ChangeAnimation);
        EventManager.Instance.Subscribe<string>("ChangeUnit", ChangeText);
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
    private void ChangeText(string param)
    {
        if (_text == null) return;
        InventoryData data = UnitManager.Instance.GetInventoryData(param);
        _text.text = data.Information;
    }
    private void ChangeAnimation(string param)
    {
        _animator.SetInteger("Unit", MatchingUnit(param));
    }
    private int MatchingUnit (string param)
    {
        switch (param)
        {
            case "King":
                return 0;
            case "Wizard":
                return 1;
            case "Pirate":
                return 2;
            case "Warrior":
                return 3;
            case "Chef":
                return 4;
            case "Archer":
                return 5;

        }
        return 0;
    }
}
