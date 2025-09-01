using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public abstract class DualButtonController<T> : MonoBehaviour
{
    protected int _index = 0;

    protected Button _prevButton;
    protected Button _nextButton;
    protected List<T> _list;


    public virtual void OnClickPrevButton()
    {
         _index--;
        ChangeButtonStatus();
    }   
    public virtual void OnClickNextButton()
    {
         _index++;
        ChangeButtonStatus();
    }
    protected abstract void SettingList();

    protected void Awake()
    {
        MatchButtons();
        SettingList();
    }

    protected virtual void OnEnable()
    {
        //SettingList();
        SettingIndex();
        ChangeButtonStatus();
    }
    protected virtual void SettingIndex()
    {
        _index = 0;
    }
    protected void ChangeButtonStatus()
    {
        bool isPrevActive = true;
        bool isNextActive = true;
        if(_index == 0)
        {
            _prevButton.gameObject.SetActive(false);
            isPrevActive = false;
        }
        if(_index>=_list.Count-1)
        {
            _nextButton.gameObject.SetActive(false);
            isNextActive = false;
        }
        
        if(isNextActive && isPrevActive) 
        {
            _prevButton.gameObject.SetActive(true);
            _nextButton.gameObject.SetActive(true);
        }
        else if(isNextActive)
        {
            _nextButton.gameObject.SetActive(true);
        }
        else if(isPrevActive)
        {
            _prevButton.gameObject.SetActive(true);
        }
    }
    protected void MatchButtons()
    {
        _prevButton = transform.Find("PrevButton").GetComponent<Button>();
        _nextButton = transform.Find("NextButton").GetComponent<Button>();
    }
}
