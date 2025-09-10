using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.U2D;

public class LaboratoryInformationController : MonoBehaviour
{
    private Image _icon;
    protected SpriteAtlas _atlas;
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
        _atlas = Resources.Load<SpriteAtlas>("UI/Image/Icon/LaboratoryIconAtlas");
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
        LaboratoryData data = LaboratoryManager.Instance.ChoiceLaboratory;
        switch (data.Effect.TargetStatus)
        {
            case TargetStatus.Skill:
                _icon.sprite = Resources.Load<Sprite>("Skills/" + data.Id);
                break;
            default:
                _icon.sprite = SpriteReturn(data.Effect.TargetStatus);
                break;
        }
    }
    protected Sprite SpriteReturn(TargetStatus type)
    {
        return _atlas.GetSprite(type.ToString());
    }

}
