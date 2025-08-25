using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public abstract class UnitButton : BaseButton
{
    protected Image _unitImage;
    protected InventoryData _data;

    public virtual InventoryData Data
    {
        get { return _data; }
        set
        {
            _data = value;
            SettingUnitImage();
        }
    }
    protected override void Awake()
    {
        base.Awake();
        Transform image = transform.Find("UnitImage");
        _unitImage = image.GetComponent<Image>();
    }
    protected void SettingUnitImage()
    {
        string path = "UI/Image/Unit/" + _data.UnitType + "/Level1Front";
        string spriteName = "Level1Front_0";

        // 모든 잘린 Sprite들을 배열로 불러온다
        Sprite[] sprites = Resources.LoadAll<Sprite>(path);
        Sprite targetSprite = sprites.FirstOrDefault(s => s.name == spriteName);
        if (targetSprite != null)
        {
            _unitImage.sprite = targetSprite;
        }
    }
}
