using System.Linq;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public abstract class UnitButton : BaseButton
{
    protected SpriteAtlas _atlas;
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
        _atlas = Resources.Load<SpriteAtlas>("UI/Image/UnitAtlas");
    }
    //protected void SettingUnitImage()
    //{
    //    string path = "UI/Image/Unit/" + _data.UnitType + "/Level1Front";
    //    string spriteName = "Level1Front_0";
    //
    //    // 모든 잘린 Sprite들을 배열로 불러온다
    //    Sprite[] sprites = Resources.LoadAll<Sprite>(path);
    //    Sprite targetSprite = sprites.FirstOrDefault(s => s.name == spriteName);
    //    if (targetSprite != null)
    //    {
    //        _unitImage.sprite = targetSprite;
    //    }
    //}
    protected void SettingUnitImage()
    {
        _unitImage.sprite = SpriteReturn(_data.UnitType);
    }
    protected Sprite SpriteReturn(EUnitType type)
    {
        return _atlas.GetSprite(type.ToString());
    }
}
