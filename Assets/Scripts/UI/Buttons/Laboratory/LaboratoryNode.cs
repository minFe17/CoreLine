using UnityEngine;
using UnityEngine.UI;

public class LaboratoryNode : BaseButton
{
    private bool _isSelect = false;
    private Image _icon;
    private LaboratoryData _data;

    protected override void Awake()
    {
        base.Awake();
        _icon = transform.Find("Icon").GetComponent<Image>();
    }
    protected override void OnClick()
    {
        if(_isSelect)
        {
            //패널끄기
            _isSelect = false;
            return;
        }
        _isSelect = true;
        //패널 켜기
    }

    
}
