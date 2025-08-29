using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    [SerializeField]
    private UpgradeType _type;

    private TextMeshProUGUI _text;
    private int _level;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
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
        switch (_type)
        {
            case UpgradeType.HealthPoint:
                _level = UnitManager.Instance.GetUnlockedUnit(type).HealthPointLevel;
                break;
            case UpgradeType.AttackRange:
                _level = UnitManager.Instance.GetUnlockedUnit(type).AttackRangeLevel;
                break;
            case UpgradeType.AttackDamage:
                _level = UnitManager.Instance.GetUnlockedUnit(type).AttackDamageLevel;
                break;
            case UpgradeType.AttackSpeed:
                _level = UnitManager.Instance.GetUnlockedUnit(type).AttackSpeedLevel;
                break;
        }

        _text.text = "+" + _level;
    }
}
