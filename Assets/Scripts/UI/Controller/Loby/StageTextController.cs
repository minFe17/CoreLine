using TMPro;
using UnityEngine;
using System;

public class StageTextController : MonoBehaviour
{
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe<StageType>("ChangeStage", ChangeText); 
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("ChangeStage", (Action<StageType>)ChangeText);
    }
    private void ChangeText(StageType type)
    {
        switch(type)
        {
            case StageType.Stage1:
                _text.text = "스테이지 1";
                break;
            case StageType.Stage2:
                _text.text = "스테이지 2";
                break;
            case StageType.Stage3:
                _text.text = "스테이지 3";
                break;
            case StageType.Infinity:
                _text.text = "무한의 탑";
                break;
        }
    }
}
