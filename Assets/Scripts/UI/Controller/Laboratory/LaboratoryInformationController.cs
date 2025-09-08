using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LaboratoryInformationController : MonoBehaviour
{
    private Image _icon;
    private TextMeshProUGUI _id;
    private TextMeshProUGUI _info;
    private LaboratoryBuyButton _button;
    private bool _isStart = false;


    private void Awake()
    {
        _icon = transform.Find("IconBackGround/Icon").GetComponent<Image>();
        _id = transform.Find("Id").GetComponent<TextMeshProUGUI>();
        _info = transform.Find("Information").GetComponent <TextMeshProUGUI>();
        _button = GetComponentInChildren<LaboratoryBuyButton>();
    }
    private void OnEnable()
    {
        UpdateInfo();
        _isStart=true;
        EventManager.Instance.Subscribe("UpdateLaboratoryInfo", UpdateInfo);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("UpdateLaboratoryInfo", (Action)UpdateInfo);
    }
    private void UpdateInfo()
    {
        if (!_isStart) return;
        _id.text = LaboratoryManager.Instance.ChoiceLaboratory.Id;
        
        
        Dictionary<string, LaboratoryData> data = LaboratoryManager.Instance.BuyLaboratoryData;

        if (data.ContainsKey(LaboratoryManager.Instance.ChoiceLaboratory.Id))
        {
            _button.gameObject.SetActive(false);
            _info.text = LaboratoryManager.Instance.ChoiceLaboratory.Info;
            return;
        }
        _button.gameObject.SetActive(true);
        _info.text = LaboratoryManager.Instance.ChoiceLaboratory.Info + "\n°¡°Ý : "+ LaboratoryManager.Instance.ChoiceLaboratory.Cost;
        SetIcon();
    }
    private void SetIcon()
    {
        string path = "UI/Image/Icon/";
        LaboratoryData data = LaboratoryManager.Instance.ChoiceLaboratory;
        switch (data.Effect.TargetStatus)
        {
            case TargetStatus.Skill:
                path = "Skills/" + data.Id;
                break;
            default:
                path += data.Effect.TargetStatus.ToString();
                break;
        }
        _icon.sprite = Resources.Load<Sprite>(path);
    }
}
