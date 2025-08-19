using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public abstract class Panel : MonoBehaviour
{
    protected Image _backGroundImage;
    protected PanelStatus _status;
    protected string _backGroundImagePath = "";
    protected List<Button> _buttons = new List<Button>();

    public void SwitchOnPanel()
    {
        gameObject.SetActive(true);
    }
    public void SwitchOffPanel()
    {
        gameObject.SetActive(false);

    }
    protected virtual void Awake()
    {
        ChangeImage();
        SwitchOffPanel();
        RegisterPanelStatus();
        UIManager.Instance.RegisterPanel(_status, this);
    }
    protected virtual void ChangeImage()
    {
        _backGroundImage = GetComponent<Image>();
        _backGroundImage.sprite = Resources.Load<Sprite>(_backGroundImagePath); //이미지 리소스 변경
    }
    protected void FindAllButtons()
    {
        _buttons.Clear(); // 기존 내용 초기화
        Button[] buttons = GetComponentsInChildren<Button>(true); // 비활성화 버튼까지 포함
        _buttons.AddRange(buttons);

        foreach (Button btn in _buttons)
        {
            btn.interactable = false;
        }
    }
    protected abstract void RegisterPanelStatus();
}
