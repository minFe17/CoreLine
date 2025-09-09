using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public abstract class PopUp : MonoBehaviour
{
    protected PopUpStatus _status;

    public PopUpStatus Status
    {
        get { return _status; }
    }
    protected virtual void Awake()
    {
        gameObject.SetActive(false);
        UIManager.Instance.RegisterPopUp(_status, this);
    }

}
