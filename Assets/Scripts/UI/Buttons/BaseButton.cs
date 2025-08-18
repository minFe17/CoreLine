using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public abstract class BaseButton : MonoBehaviour
{
    protected Image _backImage;
    protected Image _frontImage;
    protected TextMeshProUGUI _buttonText;
    protected Button _button;

    protected abstract void OnClick();
    protected virtual void Awake()
    {
        _backImage = GetComponent<Image>();
        _frontImage = GetComponentInChildren<Image>();
        _buttonText = GetComponentInChildren<TextMeshProUGUI>();
        _button = GetComponent<Button>();

        _button.onClick.AddListener(OnClick);
    }


}
