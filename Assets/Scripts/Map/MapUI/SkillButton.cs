using UnityEngine;

public class SkillButton : MonoBehaviour
{
    [SerializeField] private int _slotIndex = 0;
    [SerializeField] private SkillTargetingController _targetingController;

    private void Awake()
    {
        // Reset()은 에디터에서만 잘 호출돼요. 런타임에서도 안전하게 찾기
        if (_targetingController == null)
        {
            _targetingController = FindFirstObjectByType<SkillTargetingController>(FindObjectsInactive.Include);
        }
    }

    public void OnClick()
    {
        if (PauseControl.IsPaused) return;

        if (SkillManager.Instance == null) { Debug.LogWarning("[SkillButton] SkillManager.Instance is null"); return; }
        if (_targetingController == null) { Debug.LogWarning("[SkillButton] targetingController is null"); return; }
        if (_slotIndex < 0 || _slotIndex >= SkillManager.Instance._loadout.Count)
        { Debug.LogWarning("[SkillButton] invalid slotIndex"); return; }

        SkillManager.SelectedSkill skill = SkillManager.Instance.GetSelectedSkillBySlotIndex(_slotIndex);

        SkillTargetingSpec spec;
        SkillManager.Instance.TryGetTargetingSpec(skill, out spec);

        Debug.Log($"[SkillButton] StartTargeting slot={_slotIndex}, id={skill.Id}, mode={spec.Mode}");
        _targetingController.StartTargeting(_slotIndex, skill, spec);
    }
}
