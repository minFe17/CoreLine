using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    [SerializeField] Image _hpBar; 

    void OnEnable()
    {
        _hpBar.fillAmount = 1f;
    }

    public void ChangeHp(float hpRatio)
    {
        _hpBar.fillAmount = hpRatio;
    }
}