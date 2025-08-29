using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class NameController : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe<EUnitType>("ChangeChoiceUnitData", ChangeText);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("ChangeChoiceUnitData", (Action<EUnitType>)ChangeText);
    }
    private void ChangeText(EUnitType type)
    {
        UnlockedUnit unit = UnitManager.Instance.GetUnlockedUnit(type);
        _text.text = unit.UnitType.ToString();
    }
}
