using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LaboratoryInformationController : MonoBehaviour
{
    private Image _icon;
    private TextMeshProUGUI _id;
    private TextMeshProUGUI _info;
    private bool _isStart = false;

    public void OnClickBuy()
    {
        //구매처리
    }

    private void Awake()
    {
        _icon = transform.Find("IconBackGround/Icon").GetComponent<Image>();
        _id = transform.Find("Id").GetComponent<TextMeshProUGUI>();
        _info = transform.Find("Information").GetComponent <TextMeshProUGUI>();
    }
    private void OnEnable()
    {
        UpdateInfo();
        _isStart=true;
    }
    private void UpdateInfo()
    {
        if (!_isStart) return;
        _id.text = LaboratoryManager.Instance.ChoiceLaboratory.Id;
        _info.text = LaboratoryManager.Instance.ChoiceLaboratory.Info;
    }
}
