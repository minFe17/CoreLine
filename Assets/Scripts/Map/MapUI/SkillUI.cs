using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour
{
    private enum ArrowDefaultDirection { Up, Right, Down, Left }

    // 고정 파라미터
    private const float SlideDistance = 984f;     // 닫기: +984, 열기: -984
    private const float Duration = 0.25f;         // 슬라이드 시간
    private const float ToggleRotateZ = -90f;     // SkillControlButton만 시계 90도 회전
    private const bool StartOpen = true;          // 시작 시 열림
    private const ArrowDefaultDirection ArrowBaseDir = ArrowDefaultDirection.Up; // 화살표 원본이 '위' 기준

    // 자동 할당
    private RectTransform slideRoot;      // SkillPanel (this)
    private RectTransform toggleRect;     // SkillControlButton (회전 대상)
    private Button toggleButton;          // SkillControlButton의 Button
    private RectTransform arrowRect;      // SkillControlButton/Arrow
    private CanvasGroup panelCanvasGroup; // 있으면 사용

    private bool isOpen;
    private Vector2 openPos;
    private Vector2 closedPos;
    private Coroutine slideCo;
    private AnimationCurve ease;

    private void Awake()
    {
        // 루트
        slideRoot = GetComponent<RectTransform>();
        if (slideRoot == null)
        {
            Debug.LogError("[SkillUI] RectTransform not found on SkillPanel.");
            enabled = false; return;
        }

        // 토글 버튼 & 화살표 (계층: SkillPanel/SkillControlButton/Arrow)
        Transform toggleTf = transform.Find("SkillControlButton");
        if (toggleTf != null)
        {
            toggleRect = toggleTf as RectTransform;
            toggleButton = toggleTf.GetComponent<Button>();
        }
        Transform arrowTf = (toggleTf != null) ? toggleTf.Find("Arrow") : null;
        if (arrowTf != null) arrowRect = arrowTf as RectTransform;

        if (toggleRect == null || toggleButton == null || arrowRect == null)
        {
            Debug.LogError("[SkillUI] 'SkillControlButton'(Button/RectTransform) 또는 그 자식 'Arrow'를 찾지 못했습니다. 이름/계층을 확인하세요.");
            enabled = false; return;
        }

        // SkillControlButton만 90도 회전(스킬 버튼들은 회전 안 함!)
        Vector3 te = toggleRect.localEulerAngles;
        te.z = ToggleRotateZ;              // 시계 90°
        toggleRect.localEulerAngles = te;

        // 상태/파라미터
        panelCanvasGroup = GetComponent<CanvasGroup>(); // 있으면 사용
        ease = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        openPos = slideRoot.anchoredPosition;
        closedPos = openPos + new Vector2(SlideDistance, 0f);

        isOpen = StartOpen;
        slideRoot.anchoredPosition = isOpen ? openPos : closedPos;

        ApplyArrowRotation();  // 부모 회전 보정 포함
        ApplyCanvasGroup();

        toggleButton.onClick.AddListener(Toggle);
    }

    public void Toggle() { if (isOpen) Close(); else Open(); }

    public void Open()
    {
        isOpen = true; // 나올 때: -SlideDistance
        SlideTo(openPos);
        ApplyArrowRotation();
        ApplyCanvasGroup();
    }

    public void Close()
    {
        isOpen = false; // 들어갈 때: +SlideDistance
        SlideTo(closedPos);
        ApplyArrowRotation();
        ApplyCanvasGroup();
    }

    private void SlideTo(Vector2 target)
    {
        if (slideCo != null) StopCoroutine(slideCo);
        slideCo = StartCoroutine(CoSlide(slideRoot.anchoredPosition, target, Duration));
    }

    private IEnumerator CoSlide(Vector2 from, Vector2 to, float time)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, time);
            float k = ease.Evaluate(Mathf.Clamp01(t));
            slideRoot.anchoredPosition = Vector2.LerpUnclamped(from, to, k);
            yield return null;
        }
        slideRoot.anchoredPosition = to;
        slideCo = null;
    }

    private void ApplyArrowRotation()
    {
        // 원하는 월드 방향: 열림 →, 닫힘 ←
        float desiredWorldDeg = isOpen ? DirToDeg(ArrowDefaultDirection.Right)
                                       : DirToDeg(ArrowDefaultDirection.Left);

        // 화살표 스프라이트 기본(Up) 보정 + 부모(SkillControlButton) 회전 보정
        float baseDeg = DirToDeg(ArrowBaseDir);                // 원본 기준
        float parentZ = toggleRect.localEulerAngles.z;         // 부모의 로컬 Z(= -90)
        float localZ = desiredWorldDeg - baseDeg - parentZ;   // 자식(Arrow)의 로컬 회전값

        Vector3 e = arrowRect.localEulerAngles;
        e.z = localZ;
        arrowRect.localEulerAngles = e;
    }

    private static float DirToDeg(ArrowDefaultDirection dir)
    {
        switch (dir)
        {
            case ArrowDefaultDirection.Up: return 0f;
            case ArrowDefaultDirection.Right: return -90f;
            case ArrowDefaultDirection.Down: return 180f;
            case ArrowDefaultDirection.Left: return 90f;
        }
        return 0f;
    }

    private void ApplyCanvasGroup()
    {
        if (panelCanvasGroup == null) return;
        panelCanvasGroup.interactable = isOpen;
        panelCanvasGroup.blocksRaycasts = isOpen;
    }
}
