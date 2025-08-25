using UnityEngine;

public class UnitSettingButton : BaseButton
{
    private bool _isSelected;
    protected override void OnClick()
    {
        if (_isSelected)
        {
            EventManager.Instance.Invoke("DeleteSelectedUnit");
            return;
        }
        EventManager.Instance.Invoke("AddSelectedUnit");
    }

    private void Start()
    {
        EventManager.Instance.Subscribe("SelectUpdate", UpdateIsSelected);
        EventManager.Instance.Subscribe("DeleteSelectedUnit", UpdateIsSelected);
        EventManager.Instance.Subscribe("AddSelectedUnit", UpdateIsSelected);
    }

    private void UpdateIsSelected()
    {
        _isSelected = UnitManager.Instance.IsSelected();
        if (_isSelected)
        {
            _buttonText.text = "«ÿ¡¶";
            return;
        }
        _buttonText.text = "¿Â¬¯";
    }
}
