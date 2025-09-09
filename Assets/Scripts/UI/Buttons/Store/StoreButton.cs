using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreButton : BaseButton
{
    private Image _icon;
    private TextMeshProUGUI _quantity;
    private TextMeshProUGUI _price;
    private StoreData _data;

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
    }
    protected void Setting()
    {
        _price.text = _data.Price.ToString();
        _quantity.text = _data.Quantity.ToString();
        _icon.sprite = Resources.Load<Sprite>("UI/Image/Icon/Store/" + _data.ImageName);
        
    }


}
