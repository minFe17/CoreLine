using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public abstract class DualButtonController<T> : MonoBehaviour
{
    protected int _index = 0;
    protected Button _prevButton;
    protected Button _nextButton;

    protected List<T> _list;

    protected abstract void SettingList();

    protected void Awake()
    {
        
    }
    protected void Start()
    {
        SettingList();
    }
    protected void Update()
    {
        ChangeButtonStatus();
    }
    protected void OnEnable()
    {
        _index = 0;
    }
    protected void ChangeButtonStatus()
    {
        if(_index == 0)
        {
            _prevButton.gameObject.SetActive(false);
        }
    }
    protected void MatchButtons()
    {
        //Button[] btn = GetComponentInChildren<Button>();
    }
}
