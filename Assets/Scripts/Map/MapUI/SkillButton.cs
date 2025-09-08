using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    [Header("Auto/Manual Index")]
    [SerializeField] private bool _autoIndexByOrder = true; // 부모 안의 왼→오 순서로 자동 할당
    [SerializeField] private int _slotIndex = 0;            // _autoIndexByOrder=false일 때만 사용

    [Header("Targeting")]
    [SerializeField] private SkillTargetingController _targetingController;

    private Image _image;            // 버튼 아이콘 (자기 자신 Image)
    private int _resolvedIndex = -1; // 실제 사용할 인덱스

    private void Awake()
    {
        if (_targetingController == null)
            _targetingController = FindFirstObjectByType<SkillTargetingController>(FindObjectsInactive.Include);

        _image = GetComponent<Image>();
        SkillManager.Instance.OnLoadoutChanged += RefreshIcon;
    }

    private void OnEnable()
    {
        RefreshIndex();
        RefreshIcon();
        // Start나 다른 곳에서 로드아웃을 갱신했다면 다음 프레임에 한 번 더 동기화
        StartCoroutine(CoRefreshNextFrame());
    }

    private IEnumerator CoRefreshNextFrame()
    {
        yield return null;
        RefreshIndex();
        RefreshIcon();
    }

    // ───────── 인덱스/아이콘 갱신 ─────────
    private void RefreshIndex()
    {
        if (!_autoIndexByOrder)
        {
            _resolvedIndex = _slotIndex;
            return;
        }

        // 부모 바로 아래의 SkillButton들만 모아서 왼→오(anchoredPosition.x) 순으로 정렬
        Transform p = transform.parent;
        _resolvedIndex = _slotIndex; // 폴백

        if (p == null) return;

        List<SkillButton> siblings = new List<SkillButton>(p.childCount);
        for (int i = 0; i < p.childCount; i++)
        {
            Transform c = p.GetChild(i);
            if (c.parent != p) continue;
            SkillButton b = c.GetComponent<SkillButton>();
            if (b != null && b.isActiveAndEnabled) siblings.Add(b);
        }

        if (siblings.Count == 0) return;

        siblings.Sort((a, b) =>
        {
            RectTransform ra = a.transform as RectTransform;
            RectTransform rb = b.transform as RectTransform;
            float ax = ra != null ? ra.anchoredPosition.x : a.transform.localPosition.x;
            float bx = rb != null ? rb.anchoredPosition.x : b.transform.localPosition.x;
            return ax.CompareTo(bx);
        });

        _resolvedIndex = siblings.IndexOf(this);
    }

    private void RefreshIcon()
    {
        if (_image == null || SkillManager.Instance == null)
            return;

        // 로드아웃 범위를 벗어나면 숨김
        if (_resolvedIndex < 0 || _resolvedIndex >= SkillManager.Instance._loadout.Count)
        {
            _image.enabled = false;
            return;
        }

        // 슬롯의 스킬 ID로 아이콘 로드 (파일명 = ID, 예: Resources/SkillIcons/RangeHeal.png)
        var skill = SkillManager.Instance.GetSelectedSkillBySlotIndex(_resolvedIndex);
        Sprite icon = Resources.Load<Sprite>($"Skills/{skill.Id}");

        _image.sprite = icon;
        _image.enabled = (icon != null);
    }

    // ───────── 클릭 ─────────
    public void OnClick()
    {
        if (PauseControl.IsPaused) return;

        if (SkillManager.Instance == null) { Debug.LogWarning("[SkillButton] SkillManager.Instance is null"); return; }
        if (_targetingController == null) { Debug.LogWarning("[SkillButton] targetingController is null"); return; }

        // 인덱스 재확인(안전)
        RefreshIndex();
        if (_resolvedIndex < 0 || _resolvedIndex >= SkillManager.Instance._loadout.Count)
        { Debug.LogWarning("[SkillButton] invalid slot index"); return; }

        var skill = SkillManager.Instance.GetSelectedSkillBySlotIndex(_resolvedIndex);

        SkillTargetingSpec spec;
        SkillManager.Instance.TryGetTargetingSpec(skill, out spec);

        Debug.Log($"[SkillButton] StartTargeting slot={_resolvedIndex}, id={skill.Id}, mode={spec.Mode}");
        _targetingController.StartTargeting(_resolvedIndex, skill, spec);
    }
}
