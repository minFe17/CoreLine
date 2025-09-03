using TMPro;
using UnityEngine;

public sealed class CostUI : MonoBehaviour
{
    [Header("Texts (있으면 갱신, 비워두면 무시)")]
    [SerializeField] private TMP_Text _unitText;   // 유닛 코스트 표시용
    [SerializeField] private TMP_Text _skillText;  // 스킬 코스트 표시용

    [Header("Formats")]
    [SerializeField] private string _unitNumberFormat = "0";
    [SerializeField] private string _skillNumberFormat = "0";

    private CostManager cost;

    private void Reset()
    {
        // 이름으로 자동 바인딩 시도 (선택)
        if (_unitText == null) _unitText = FindChildText("UnitCostText");
        if (_skillText == null) _skillText = FindChildText("SkillCostText");
    }

    private TMP_Text FindChildText(string childName)
    {
        Transform t = transform.Find(childName);
        return t ? t.GetComponent<TMP_Text>() : GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        cost = CostManager.Instance;

        // 초기 표시
        if (_unitText != null) _unitText.text = cost.CurrentUnit.ToString(_unitNumberFormat);
        if (_skillText != null) _skillText.text = cost.CurrentSkill.ToString(_skillNumberFormat);

        // 필요한 쪽만 구독
        if (_unitText != null) cost.OnUnitChanged += OnUnitChanged;
        if (_skillText != null) cost.OnSkillChanged += OnSkillChanged;
    }

    private void OnDisable()
    {
        if (cost == null) return;
        if (_unitText != null) cost.OnUnitChanged -= OnUnitChanged;
        if (_skillText != null) cost.OnSkillChanged -= OnSkillChanged;
    }

    private void OnUnitChanged(int value)
    {
        if (_unitText != null) _unitText.text = value.ToString(_unitNumberFormat);
    }

    private void OnSkillChanged(int value)
    {
        if (_skillText != null) _skillText.text = value.ToString(_skillNumberFormat);
    }

    // 필요 시 런타임에 참조 교체 가능
    public void SetUnitText(TMP_Text text)
    {
        if (cost != null && _unitText == null) cost.OnUnitChanged += OnUnitChanged;
        _unitText = text;
        if (_unitText != null) _unitText.text = cost.GetCurrent(CostManager.CostType.Unit).ToString(_unitNumberFormat);
    }

    public void SetSkillText(TMP_Text text)
    {
        if (cost != null && _skillText == null) cost.OnSkillChanged += OnSkillChanged;
        _skillText = text;
        if (_skillText != null) _skillText.text = cost.GetCurrent(CostManager.CostType.Skill).ToString(_skillNumberFormat);
    }
}
