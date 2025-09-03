using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Utils;
using UnityEngine.U2D;

public class TowerPlaceButton : BaseButton
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _costText;

    private TowerOption _option;
    private Action<TowerOption> _onPick;

    private EUnitType _unitType;

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

    public void Bind(EUnitType option, Action<TowerOption> onPick)
    {
        _unitType = option;

        if (_icon != null)
        {
            _icon.sprite = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.SpriteAtlas).GetPrefabAtlas(EAtlasPrefabType.UnitSpriteAtlas).GetSprite(_unitType.ToString());
        }

        if (_costText != null)
            _costText.text = SimpleSingleton<UnitDataList>.Instance.GetUnitData(_unitType).LevelData[0].Cost.ToString();
    }

    protected override void OnClick()
    {
        _onPick?.Invoke(_option);
        MonoSingleton<GameStateManager>.Instance.CreateUnit(_unitType);
    }
}
