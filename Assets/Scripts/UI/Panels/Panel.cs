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
        _backGroundImage.sprite = Resources.Load<Sprite>(_backGroundImagePath); //�̹��� ���ҽ� ����
    }
    protected void FindAllButtons()
    {
        _buttons.Clear(); // ���� ���� �ʱ�ȭ
        Button[] buttons = GetComponentsInChildren<Button>(true); // ��Ȱ��ȭ ��ư���� ����
        _buttons.AddRange(buttons);

        foreach (Button btn in _buttons)
        {
            btn.interactable = false;
        }
    }
    protected abstract void RegisterPanelStatus();
}
