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
        //인앱구매하고 돈 추가해주는거까지 추가해야됨.
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
