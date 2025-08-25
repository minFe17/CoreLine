using System.Linq;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUnitButton : UnitButton
{
    private Image _buyImage;


    public override InventoryData Data
    {
        get { return _data; }
        set
        {
            base.Data = value;
            SettingBuyImage();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Transform trans = transform.Find("BuyImage");
        _buyImage = trans.GetComponent<Image>();
    }
    private void Start()
    {
        EventManager.Instance.Subscribe("SettingBuyUnit", SettingBuyImage); 
        //이거 좀 비효율적인것같은데 고민한번 해보자
        
    }
    protected override void OnClick()
    {
        EventManager.Instance.Invoke<EUnitType>("ChangeUnit", _data.UnitType);
    }
    private void SettingBuyImage()
    {
        GameData data = DataManager.Instance.GameData;

        foreach (UnlockedUnit unit in data.UnlockedUnit)
        {
            if (unit.UnitType == _data.UnitType)
            {
                _buyImage.gameObject.SetActive(false);
                return;
            }
        }
    }
}
