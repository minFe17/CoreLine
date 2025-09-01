using TMPro;
using UnityEngine;

public class CostUI : MonoBehaviour
{
    [SerializeField] private TMP_Text costText;
    [SerializeField] private string numberFormat = "0"; 

    CostManager _cost;

    void OnEnable()
    {
        _cost = CostManager.Instance;                         // 필요 시 자동 생성
        if (costText) costText.text = _cost.Current.ToString(numberFormat);
        _cost.OnChanged += OnChanged;
    }

    void OnDisable()
    {
        if (_cost != null) _cost.OnChanged -= OnChanged;
    }

    void OnChanged(int v)
    {
        if (costText) costText.text = v.ToString(numberFormat);
    }
}
