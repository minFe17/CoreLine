using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Panel : MonoBehaviour
{
    [SerializeField]
    protected PanelStatus _status;

    protected List<Button> _buttons = new List<Button>();

    public PanelStatus Status
    {
        get { return _status; }
    }
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
        SwitchOffPanel();
        UIManager.Instance.RegisterPanel(_status, this);
    }
    protected virtual void Start()
    {
        if (_status == PanelStatus.LobyPanel) return;
        GameObject exitButton = Resources.Load<GameObject>("UI/Prefabs/Button/Share/ExitButton");
        GameObject newButton = Instantiate(exitButton);
        newButton.transform.SetParent(this.transform, false);
    }


    protected void FindAllButtons()
    {
        //이건 팝업이랑 엮을때 필요한거임
        _buttons.Clear(); // 기존 내용 초기화
        Button[] buttons = GetComponentsInChildren<Button>(true); // 비활성화 버튼까지 포함
        _buttons.AddRange(buttons);

        foreach (Button btn in _buttons)
        {
            btn.interactable = false;
        }
    }
}
