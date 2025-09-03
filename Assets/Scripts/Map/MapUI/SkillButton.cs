using UnityEngine;

public class SkillButton : MonoBehaviour
{
    [SerializeField] private int slotIndex = 0;
    [SerializeField] private SkillTargetingController targetingController;

    private void Awake()
    {
        // Reset()은 에디터에서만 잘 호출돼요. 런타임에서도 안전하게 찾기
        if (targetingController == null)
        {
            targetingController = FindFirstObjectByType<SkillTargetingController>(FindObjectsInactive.Include);
        }
    }

    public void OnClick()
    {
        if (SkillManager.Instance == null) { Debug.LogWarning("[SkillButton] SkillManager.Instance is null"); return; }
        if (targetingController == null) { Debug.LogWarning("[SkillButton] targetingController is null"); return; }
        if (slotIndex < 0 || slotIndex >= SkillManager.Instance.loadout.Count)
        { Debug.LogWarning("[SkillButton] invalid slotIndex"); return; }

        SkillManager.SelectedSkill skill = SkillManager.Instance.GetSelectedSkillBySlotIndex(slotIndex);

        SkillTargetingSpec spec;
        SkillManager.Instance.TryGetTargetingSpec(skill, out spec);

        Debug.Log($"[SkillButton] StartTargeting slot={slotIndex}, id={skill.Id}, mode={spec.Mode}");
        targetingController.StartTargeting(slotIndex, skill, spec);
    }
}
