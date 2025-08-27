using System.Linq;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUnitButton : UnitButton
{
    private bool _isBuy = false;
    private Image _buyImage;
    private TextMeshProUGUI _buyText;
    private UIColorApplier _colorApplier;
    private ColorType _originalColorType;
 


    public override InventoryData Data
    {
        get { return _data; }
        set
        {
            _data = value;
            if (_data.UnitType == UnitManager.Instance.ChoiceUnit.UnitType)
                ChangeColorType(this.gameObject);
            SettingBuyImage();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Transform trans = transform.Find("BuyImage");
        _buyImage = trans.GetComponent<Image>();
        _buyText = trans.GetComponentInChildren<TextMeshProUGUI>();
        _colorApplier = GetComponent<UIColorApplier>();
        _originalColorType = _colorApplier.MyColorType;
    }
    private void Start()
    {
        EventManager.Instance.Subscribe<GameObject>("ChangeColorTypeToInventoryUnitButton", ChangeColorType);
    }
    protected override void OnClick()
    {
        EventManager.Instance.Invoke<EUnitType>("ChangeChoiceUnitData", _data.UnitType);
        EventManager.Instance.Invoke<bool,EUnitType>("IsBuyUnit", _isBuy,_data.UnitType);
        EventManager.Instance.Invoke<GameObject>("ChangeColorTypeToInventoryUnitButton", this.gameObject);
    }
    private void SettingBuyImage()
    {
        GameData data = DataManager.Instance.GameData;

        foreach (UnlockedUnit unit in data.UnlockedUnit)
        {
            if (unit.UnitType == _data.UnitType)
            {
                _buyImage.color = new Color(0.4f, 0.8f, 0.5f);
                _buyText.text = Data.UnitType.ToString();
                SettingUnitImage();
                _isBuy = true;
                return;
            }
        }
       
    }
    private void ChangeColorType(GameObject obj)
    {
        if (obj == gameObject)
        {
            _colorApplier.MyColorType = ColorType.Light;
        }
        else
        {
            _colorApplier.MyColorType = _originalColorType;
        }
    }
}
