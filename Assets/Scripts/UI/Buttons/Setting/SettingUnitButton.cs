using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SettingUnitButton : BaseButton //UnitButton으로 빼서 상속해서 쓰기
{
    private Image _unitImage;
    private InventoryData _data;

    public InventoryData Data
    {
        get { return _data; }
        set 
        {
            _data = value;
            SettingImage();
        }
    }

    protected override void Awake()
    {
        base.Awake();
        Transform trans = transform.Find("UnitImage");
        _unitImage = trans.GetComponent<Image>();
    }
    protected override void OnClick()
    {
        EventManager.Instance.Invoke<string>("ChangeUnit", _data.UnitType);
    }
    private void SettingImage()
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
