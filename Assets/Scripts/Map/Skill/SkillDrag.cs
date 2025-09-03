using UnityEngine;
using UnityEngine.EventSystems;

public class SkillDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private int slotIndex = 0;                        // 이 버튼의 스킬 슬롯
    [SerializeField] private RectTransform cancelZonePanel;            // 드래그를 이 패널 위에 놓으면 취소
    [SerializeField] private SkillTargetingController targetingCtrl;   // 씬의 컨트롤러 (없으면 자동탐색)

    private Camera uiCam; // 이벤트 카메라

    private void Awake()
    {
        if (targetingCtrl == null)
#if UNITY_2023_1_OR_NEWER
            targetingCtrl = FindFirstObjectByType<SkillTargetingController>(FindObjectsInactive.Include);
#else
            targetingCtrl = FindObjectOfType<SkillTargetingController>(true);
#endif
        if (cancelZonePanel == null)
        {
            // 버튼이 속한 패널을 기본 취소존으로 사용 (필요시 Inspector로 교체)
            cancelZonePanel = GetComponentInParent<RectTransform>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        uiCam = eventData.pressEventCamera;

        if (SkillManager.Instance == null || targetingCtrl == null) return;
        if (slotIndex < 0 || slotIndex >= SkillManager.Instance.loadout.Count) return;

        SkillManager.SelectedSkill skill = SkillManager.Instance.GetSelectedSkillBySlotIndex(slotIndex);

        SkillTargetingSpec spec;
        SkillManager.Instance.TryGetTargetingSpec(skill, out spec);

        targetingCtrl.StartTargetingDrag(slotIndex, skill, spec);           // 드래그 주도 모드로 시작
        targetingCtrl.UpdateDragScreenPosition(eventData.position, uiCam);   // 첫 위치 업데이트
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (targetingCtrl == null) return;
        targetingCtrl.UpdateDragScreenPosition(eventData.position, eventData.pressEventCamera);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (targetingCtrl == null) return;

        bool droppedOnCancel = false;
        if (cancelZonePanel != null)
        {
            droppedOnCancel = RectTransformUtility.RectangleContainsScreenPoint(
                cancelZonePanel, eventData.position, eventData.pressEventCamera);
        }

        if (droppedOnCancel)
        {
            targetingCtrl.CancelFromUI();
        }
        else
        {
            targetingCtrl.CommitFromScreen(eventData.position); // 드랍 위치로 시전
        }
    }
}
