using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [SerializeField] Image _hpBar;
    Vector3 _hpBarPos = new Vector3(0, -0.2f, 0);

    void OnEnable()
    {
        _hpBar.fillAmount = 1f;
    }

    public void SetPosition(Vector3 unitPosition)
    {
        GetComponent<RectTransform>().position = _hpBarPos + unitPosition;
    }

    public void ChangeHp(float hpRatio)
    {
        _hpBar.fillAmount = hpRatio;
    }
}