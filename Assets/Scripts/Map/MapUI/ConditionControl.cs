using UnityEngine;

public static class ConditionControl
{
    public static NormalStageManager.StageEndSnapshot BuildFor(in NormalStageData stage)
    {
        var snap = new NormalStageManager.StageEndSnapshot();

        if (stage.Condition == null || stage.Condition.Count == 0)
            return snap;

        // 중복 계산 방지 플래그 (필요한 것만 한 번 계산)
        bool moneyDone = false;
        bool baseHpDone = false;
        bool unitDestroyedDone = false;

        for (int i = 0; i < stage.Condition.Count; i++)
        {
            var cond = stage.Condition[i];
            switch (cond.ClearType)
            {
                case ClearType.MoneySave:
                    if (!moneyDone)
                    {
                        snap.moneyLeft = CostManager.Instance ? CostManager.Instance.CurrentUnit : 0;
                        moneyDone = true;
                    }
                    break;

                case ClearType.HealthSave:
                    if (!baseHpDone)
                    {
                        // 프로젝트의 실제 베이스 HP 비율로 교체
                        // 예: PlayerBase.Instance ? PlayerBase.Instance.HpRatio : 1f
                        snap.baseHpRatio = 1f;
                        baseHpDone = true;
                    }
                    break;

                case ClearType.UnitSave:
                    if (!unitDestroyedDone)
                    {
                        // 프로젝트의 실제 파괴 유닛 수로 교체
                        // 예: TowerManager.Instance != null ? TowerManager.Instance.DestroyedCount : 0
                        snap.unitDestroyedCount = 0;
                        unitDestroyedDone = true;
                    }
                    break;

                    // 새 조건이 생기면 여기 case 하나만 추가하면 됩니다.
                    // case ClearType.YourNewRule:
                    //     if (!flag) { snap.yourField = ...; flag = true; }
                    //     break;
            }
        }

        return snap;
    }
}
