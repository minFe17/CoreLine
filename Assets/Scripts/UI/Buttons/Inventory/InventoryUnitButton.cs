using System.Linq;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUnitButton : BaseButton
{
    private Image _unitImage;
    private Image _buyImage;
    private InventoryData _data;
    
    public InventoryData Data
    {
        get { return _data; }
        set 
        { 
            _data = value;
            SettingBuyImage();
            SettingUnitImage();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Transform trans = transform.Find("BuyImage");
        _buyImage = trans.GetComponent<Image>();
        trans = transform.Find("UnitImage");
        _unitImage = trans.GetComponent<Image>();
    }
    protected override void OnClick()
    {
        print(_data.UnitType);
        UnitManager.Instance.ChoiceUnit = _data;
        print(UnitManager.Instance.ChoiceUnit.UnitType);
        EventManager.Instance.Invoke<string>("ChangeUnit", _data.UnitType);
    }
    private void SettingBuyImage()
    {
        GameData data = DataManager.Instance.GameData;

        foreach (UnlockedUnit unit in data.UnlockedUnit)
        {
            if (unit.Type == _data.UnitType)
            {
                _buyImage.gameObject.SetActive(false);
            }
        }
    }
    private void SettingUnitImage()
    {
        string path = "UI/Image/Unit/" + _data.UnitType + "/Level1Front";
        string spriteName = "Level1Front_0";

        // 모든 잘린 Sprite들을 배열로 불러온다
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        Sprite targetSprite = sprites.FirstOrDefault(s => s.name == spriteName);
        print(path);
        if (targetSprite != null)
        {
            _unitImage.sprite = targetSprite;
        }
    }
}
