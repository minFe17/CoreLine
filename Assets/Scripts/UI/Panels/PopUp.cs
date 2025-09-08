using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PopUp : MonoBehaviour
{
    [SerializeField]
    protected PopUpStatus _status;

    public PopUpStatus Status
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
        UIManager.Instance.RegisterPopUp(_status, this);
    }

}
