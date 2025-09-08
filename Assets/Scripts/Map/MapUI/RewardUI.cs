using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class RewardItemUI : MonoBehaviour
{
    private Image _icon;        // 자식: "RewardIcon"
    private TMP_Text _amount;   // 자식: "RewardText"

    private void Awake()
    {
        // 이름 기준 자동 배선 (프리팹에서 위치/사이즈는 네가 설정한 그대로 유지)
        Transform tIcon = transform.Find("RewardIcon");
        if (tIcon) _icon = tIcon.GetComponent<Image>();
        if (_icon == null) _icon = GetComponentInChildren<Image>(true);

        Transform tText = transform.Find("RewardText");
        if (tText) _amount = tText.GetComponent<TMP_Text>();
        if (_amount == null) _amount = GetComponentInChildren<TMP_Text>(true);
    }

    public void Set(Sprite sprite, int value)
    {
        if (_icon)
        {
            _icon.sprite = sprite;
            _icon.enabled = (sprite != null); // 아이콘 없으면 흰 사각형 방지
        }
        if (_amount) _amount.text = value.ToString("N0");
    }

    // 아이콘을 ID로 찾고 싶을 때(파일명=ID, Resources/Reward/{ID}.png 같은 구조)
    public void SetById(string id, int value)
    {
        Sprite s = NormalStageManager.Instance != null
            ? NormalStageManager.Instance.GetRewardIcon(id)
            : null;
        Set(s, value);
    }

#if UNITY_EDITOR
    private void Reset() { Awake(); } // 프리팹에서 Reset 눌러도 자동 배선되게
#endif
}
