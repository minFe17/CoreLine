using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using Utils;

public abstract class BaseButton : MonoBehaviour
{
    protected Image _backImage;
    protected TextMeshProUGUI _buttonText;
    protected Button _button;

    protected virtual void OnClick()
    {
        MonoSingleton<AudioClipManager>.Instance.PlaySFX(ESFXType.UI_ButtonClick);
    }
    protected virtual void Awake()
    {
        SettingComponent();
        _button.onClick.AddListener(OnClick);
    }
    protected void SettingComponent()
    {
        _backImage = GetComponent<Image>();
        _buttonText = GetComponentInChildren<TextMeshProUGUI>();
        _button = GetComponent<Button>();

    }

}
