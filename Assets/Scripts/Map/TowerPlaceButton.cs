using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TowerPlaceButton : BaseButton
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _costText;

    private TowerOption _option;
    private Action<TowerOption> _onPick;

    // BuildUI에서 바인딩할 때 호출
    public void Bind(TowerOption option, Action<TowerOption> onPick)
    {
        _option = option;
        _onPick = onPick;

        if (_icon != null)
        {
            _icon.sprite = option.Icon;
            _icon.enabled = (option.Icon != null);
        }

        if (_costText != null)
            _costText.text = option.Cost.ToString();
    }

    protected override void OnClick()
    {
        _onPick?.Invoke(_option);
    }
}
