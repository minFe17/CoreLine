using UnityEngine;
using UnityEngine.UI;
using static SkillManager;

public class SkillSelectButton : BaseButton
{
    private bool _isSetting = false;
    private Image _icon;
    private LaboratoryData _data;

    public bool IsSetting
    {
        get { return _isSetting; }
        set { _isSetting = value; }
    }
    public LaboratoryData Data
    {
        get { return _data; }
        set 
        { 
            _data = value;
            SettingButton();
        }
    }
    protected override void Awake()
    {
        base.Awake();
        _icon = transform.Find("Icon").GetComponent<Image>();
    }
    protected override void OnClick()
    {
        base.OnClick();
        EventManager.Instance.Invoke<SkillSelectButton>("ChoiceSkillButton", this);
    }
    private void SettingButton()
    {
        //이미지 셋팅해주기(스킬마다 이미지 이름으로 빼놓자!)
        _icon.sprite = Resources.Load<Sprite>("Skills/"+_data.Id);
    }
}
