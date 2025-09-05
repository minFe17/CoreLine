using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class ClearPanelControl : MonoBehaviour
{
    [Header("Roots")]
    [SerializeField] private RectTransform panelRoot;   // ClearPanel (this) 넣기 권장
    [SerializeField] private RectTransform starGroup;   // ClearStar 컨테이너
    [Tooltip("0.5=정중앙, 1.0=최상단. 중앙~상단 사이 원하는 위치로 배치")]
    [SerializeField, Range(0.5f, 1f)] private float starYBetweenCenterAndTop = 0.72f;
    [SerializeField] private bool autoRepositionOnResize = true;

    [Header("Stars (StarUI 프리팹 3개)")]
    [SerializeField] private List<StarUI> stars = new List<StarUI>(3);
    [SerializeField] private float delayBetweenStars = 0.1f; // 순차 연출 간 딜레이(초), 0이면 동시

    [Header("Conditions UI (선택)")]
    [SerializeField] private RectTransform conditionsRoot; // VerticalLayoutGroup 있는 컨테이너
    [SerializeField] private GameObject conditionRowPrefab; // Text + Icon 포함 프리팹(선택)
    [SerializeField] private Sprite checkIcon;  // 충족 아이콘
    [SerializeField] private Sprite crossIcon;  // 미충족 아이콘
    [SerializeField] private Color checkColor = Color.white;
    [SerializeField] private Color crossColor = new Color(0.7f, 0.7f, 0.7f, 1f);

    private readonly List<GameObject> _spawnedRows = new List<GameObject>();

    private void Reset()
    {
        if (panelRoot == null) panelRoot = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        if (autoRepositionOnResize) RepositionStars();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (autoRepositionOnResize && isActiveAndEnabled) RepositionStars();
    }

    /// <summary>ClearStar를 화면 중앙~상단 사이에 배치</summary>
    private void RepositionStars()
    {
        if (panelRoot == null || starGroup == null) return;

        starGroup.anchorMin = starGroup.anchorMax = new Vector2(0.5f, 0.5f);
        starGroup.pivot = new Vector2(0.5f, 0.5f);

        float parentH = panelRoot.rect.height;
        float norm = Mathf.Clamp(starYBetweenCenterAndTop, 0.5f, 1f);
        float yFromCenter = (norm - 0.5f) * parentH;
        starGroup.anchoredPosition = new Vector2(0f, yFromCenter);
    }

    // ─────────────────────────────────────────────────────────────────────
    // 공개 API
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// NormalStageManager.LastResult에 저장된 마지막 결과를 기반으로 표시.
    /// (클리어 직후 패널을 띄울 때 간편)
    /// </summary>
    public void ShowFromLastResult()
    {
        var maybe = NormalStageManager.Instance.LastResult;
        if (!maybe.HasValue)
        {
            Debug.LogWarning("[ClearPanelController] LastResult 없음");
            return;
        }
        var last = maybe.Value;
        Show(last.Stage, last.Snapshot, last.Stars);
    }

    /// <summary>
    /// 스테이지/스냅샷을 받아 패널을 연출/표시.
    /// starsEarned=-1이면 내부에서 평가함.
    /// </summary>
    public void Show(NormalStageData stage, StageEndSnapshot snap, int starsEarned = -1)
    {
        if (stage.Condition == null) stage.Condition = new List<Condition>();

        if (starsEarned < 0)
            starsEarned = NormalStageManager.Instance.EvaluateStars(stage, snap);

        int maxStars = Mathf.Min(3, stage.Condition.Count);
        int met = Mathf.Clamp(starsEarned, 0, maxStars);

        // 별 초기화 & 표시
        for (int i = 0; i < stars.Count; i++)
        {
            bool active = i < maxStars;
            stars[i].gameObject.SetActive(active);
            if (active) stars[i].InitIdle();
        }
        StartCoroutine(CoPlayStars(met, maxStars));

        // 조건 리스트(선택)
        if (conditionsRoot != null)
            RebuildConditions(stage, snap);

        // 배치 갱신
        RepositionStars();

        // 패널 활성화 (이 스크립트를 ClearPanel에 붙였으면 외부에서 SetActive(true)로 켜도 됨)
        if (!gameObject.activeSelf) gameObject.SetActive(true);
    }

    // ─────────────────────────────────────────────────────────────────────
    // 내부 로직
    // ─────────────────────────────────────────────────────────────────────

    private IEnumerator CoPlayStars(int met, int maxStars)
    {
        for (int i = 0; i < stars.Count && i < maxStars; i++)
        {
            bool success = i < met;
            stars[i].PlayCondition(success);           // StarUI가 fillAmount만 애니메이션
            if (delayBetweenStars > 0f)
                yield return new WaitForSecondsRealtime(delayBetweenStars);
        }
    }

    private void RebuildConditions(NormalStageData stage, StageEndSnapshot snap)
    {
        // 기존 행 제거
        for (int i = 0; i < _spawnedRows.Count; i++) Destroy(_spawnedRows[i]);
        _spawnedRows.Clear();

        int count = Mathf.Min(3, stage.Condition.Count);
        for (int i = 0; i < count; i++)
        {
            Condition cond = stage.Condition[i];
            bool met = IsConditionMet(cond, snap);

            GameObject row = conditionRowPrefab != null
                ? Instantiate(conditionRowPrefab, conditionsRoot)
                : new GameObject("CondRow", typeof(RectTransform));

            _spawnedRows.Add(row);

            // 라벨
            TMP_Text label = row.GetComponentInChildren<TMP_Text>();
            if (label == null) label = row.AddComponent<TextMeshProUGUI>();
            label.text = cond.Info;

            // 아이콘
            Image icon = null;
            Image[] imgs = row.GetComponentsInChildren<Image>(true);
            if (imgs != null && imgs.Length > 0) icon = imgs[imgs.Length - 1];
            if (icon == null) icon = row.AddComponent<Image>();
            icon.sprite = met ? checkIcon : crossIcon;
            icon.color = met ? checkColor : crossColor;
        }
    }

    private bool IsConditionMet(Condition c, StageEndSnapshot s)
    {
        switch (c.ClearType)
        {
            case ClearType.MoneySave: return s.moneyLeft >= c.Value;
            case ClearType.HealthSave: return s.baseHpRatio >= c.Value; // 0~1
            case ClearType.UnitSave: return s.unitDestroyedCount < c.Value;
            default: return false;
        }
    }
}
