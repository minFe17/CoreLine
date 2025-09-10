using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ShowMoneyController : MonoBehaviour
{
    private TextMeshProUGUI _money;
    private TextMeshProUGUI _gem;
    private TextMeshProUGUI _key;

    private void Awake()
    {
        _money =  transform.Find("MoneyImage/MoneyText").GetComponent<TextMeshProUGUI>();
        _gem = transform.Find("GemImage/GemText").GetComponent<TextMeshProUGUI>();
        _key = transform.Find("InfinityKeyImage/KeyText").GetComponent <TextMeshProUGUI>();
    }
    private void Start()
    {
        UpdateText();
    }
    private void OnEnable()
    {
        EventManager.Instance.Subscribe("UpdateMoneyText", UpdateText);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("UpdateMoneyText", (Action)UpdateText);
    }
    private void UpdateText()
    {
        _money.text = DataManager.Instance.GameData.PlayerMoney.ToString();
        _gem.text = DataManager.Instance.GameData.PlayerGem.ToString() ;
        _key.text = DataManager.Instance.GameData.PlayerInfinityKey.ToString() ;
    }
}
