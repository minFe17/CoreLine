using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Utils;

public class PopUp : MonoBehaviour
{
    [SerializeField]
    protected PopUpStatus _status;

    public PopUpStatus Status
    {
        get { return _status; }
    }
    public void SwitchOn()
    {
        gameObject.SetActive(true);

        if (_status == PopUpStatus.WaitAlret) return;
        MonoSingleton<AudioClipManager>.Instance.PlaySFX(ESFXType.UI_PopUp);
    }
    public void SwitchOff()
    {
        //MonoSingleton<AudioClipManager>.Instance.StopSFX();
        gameObject.SetActive(false);
    }
    protected virtual void Awake()
    {
        gameObject.SetActive(false);
        UIManager.Instance.RegisterPopUp(_status, this);
    }
    protected virtual void OnDestroy()
    {
        UIManager.Instance.UnregisterPopUp(_status);
    }
}
