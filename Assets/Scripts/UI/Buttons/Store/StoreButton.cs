using TMPro;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class StoreButton : BaseButton
{
    private Image _icon;
    private TextMeshProUGUI _quantity;
    private TextMeshProUGUI _price;
    private StoreData _data;
    protected SpriteAtlas _atlas;

    public StoreData Data
    {
        get { return _data; }
        set 
        { 
            _data = value;
            Setting();
        }
    }
    protected override void OnClick()
    {
        base.OnClick();
        switch (_data.StoreType)
        {
            case StoreType.Money:
                DataManager.Instance.GameData.PlayerMoney += _data.Quantity;
                break;
            case StoreType.InfinityKey:
                DataManager.Instance.GameData.PlayerInfinityKey += _data.Quantity;
                break;
            case StoreType.Gem:
                DataManager.Instance.GameData.PlayerGem += _data.Quantity;
                break;
        }
        EventManager.Instance.Invoke("UpdateMoneyText");
    }
    protected override void Awake()
    {
        base.Awake();
        _icon = transform.Find("Icon").GetComponent<Image>();
        _quantity = transform.Find("Quantity").GetComponent<TextMeshProUGUI>();
        _price = transform.Find("BuyButton/Price").GetComponent <TextMeshProUGUI>();
        _atlas = Resources.Load<SpriteAtlas>("UI/Image/Icon/Store/ShopIconAtlas");
    }
    protected void Setting()
    {
        _price.text = _data.Price.ToString();
        _quantity.text = _data.Quantity.ToString();
        _icon.sprite = SpriteReturn(_data.ImageName);
        
    }
    protected Sprite SpriteReturn(string name)
    {
        return _atlas.GetSprite(name);
    }


}
