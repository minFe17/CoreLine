using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private int _slotIndex = 0;                        // 이 버튼의 스킬 슬롯
    [SerializeField] private SkillTargetingController _targetingCtrl;   // 씬의 컨트롤러 (없으면 자동탐색)

    private RectTransform _cancelZonePanel; // 자동 탐색으로 세팅
    private Camera uiCam; // 이벤트 카메라

    private void Awake()
    {
        if (_targetingCtrl == null)
            _targetingCtrl = Object.FindFirstObjectByType<SkillTargetingController>(FindObjectsInactive.Include);

        ResolveCancelZone(); // 스킬 패널을 취소존으로 자동 탐색
    }

    // 스킬 패널(취소존) 자동 탐색
    private void ResolveCancelZone()
    {
        if (_cancelZonePanel != null) return;

        // 1) 이름 기반 우선 탐색 (필요시 원하는 이름 추가)
        _cancelZonePanel = FindAncestorByName(transform, new[] { "SkillPanel", "SkillsPanel", "SkillBar", "SkillButtonPanel" });
        if (_cancelZonePanel) return;

        // 2) 레이아웃 그룹(버튼 컨테이너) 탐색
        var h = GetComponentInParent<HorizontalLayoutGroup>(true);
        if (h != null) { _cancelZonePanel = h.GetComponent<RectTransform>(); return; }
        var v = GetComponentInParent<VerticalLayoutGroup>(true);
        if (v != null) { _cancelZonePanel = v.GetComponent<RectTransform>(); return; }
        var anyLayout = GetComponentInParent<LayoutGroup>(true);
        if (anyLayout != null) { _cancelZonePanel = anyLayout.GetComponent<RectTransform>(); return; }

        // 3) 마지막으로, 같은 Canvas 아래 가장 가까운 상위 패널
        _cancelZonePanel = GetNearestPanelUnderSameCanvas(transform as RectTransform);
    }

    private static RectTransform FindAncestorByName(Transform tr, string[] names)
    {
        Transform cur = tr;
        while (cur != null)
        {
            for (int i = 0; i < names.Length; i++)
                if (string.Equals(cur.name, names[i], System.StringComparison.OrdinalIgnoreCase))
                    return cur as RectTransform;
            cur = cur.parent;
        }
        return null;
    }

    private static RectTransform GetNearestPanelUnderSameCanvas(RectTransform child)
    {
        if (child == null) return null;
        var canvas = child.GetComponentInParent<Canvas>(true);
        Transform cur = child.transform;
        RectTransform candidate = null;

        while (cur != null && cur.parent is RectTransform prt)
        {
            if (canvas != null && prt == canvas.transform) break; // Canvas 바로 아래까지만
            candidate = prt;
            cur = prt;
        }
        return candidate;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (PauseControl.IsPaused) return;

        if (_targetingCtrl == null)
            _targetingCtrl = FindFirstObjectByType<SkillTargetingController>(FindObjectsInactive.Include);
        if (_targetingCtrl == null || SkillManager.Instance == null) return;

        if (_slotIndex < 0 || _slotIndex >= SkillManager.Instance._loadout.Count) return;

        var skill = SkillManager.Instance.GetSelectedSkillBySlotIndex(_slotIndex);
        SkillTargetingSpec spec; SkillManager.Instance.TryGetTargetingSpec(skill, out spec);

        _targetingCtrl.StartTargetingDrag(_slotIndex, skill, spec, eventData.position, eventData.pressEventCamera);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (PauseControl.IsPaused) return;
        if (_targetingCtrl == null) return;

        _targetingCtrl.UpdateDragScreenPosition(eventData.position, eventData.pressEventCamera);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (PauseControl.IsPaused) return;
        if (_targetingCtrl == null) return;

        bool droppedOnCancel = false;
        if (_cancelZonePanel != null)
        {
            droppedOnCancel = RectTransformUtility.RectangleContainsScreenPoint(
                _cancelZonePanel, eventData.position, eventData.pressEventCamera);
        }

        if (droppedOnCancel)
            _targetingCtrl.CancelFromUI();
        else
            _targetingCtrl.CommitFromScreen(eventData.position);
    }
}
