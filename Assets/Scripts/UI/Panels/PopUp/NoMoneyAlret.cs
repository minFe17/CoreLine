using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class NoMoneyAlret : PopUp
{
    public void OnClickStore()
    {
        UIManager.Instance.AddPanelStack(PanelStatus.StorePanel);
        UIManager.Instance.ClosePopUp();
    }
}

