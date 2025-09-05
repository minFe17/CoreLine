using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public sealed class NormalStageManager : SimpleSingleton<NormalStageManager>
{
    private NormalStageData _selectedStage; //선택된 스테이지 데이터
    // 진행/보상 갱신되면 UI가 이것을 구독해서 갱신하면 편함
    public event Action OnStageProgressChanged;
    public struct LastRun
    {
        public NormalStageData Stage;
        public StageEndSnapshot Snapshot;
        public int Stars;
        public RewardResult Reward;
    }

    private LastRun? _lastResult;

    public NormalStageManager()
    {
        EventManager.Instance.Subscribe<NormalStageData>("SelectStage", SelectStage);
        //EventManager로 여러 함수 작동
    }

    public LastRun? LastResult => _lastResult;

    //외부에서 접근 가능한 Get
    public NormalStageData SelectedStage
    {
        get { return _selectedStage; }
    } 

    public void SetLastResult(in NormalStageData stage, in StageEndSnapshot snap, int stars, in RewardResult reward)
    {
        _lastResult = new LastRun { Stage = stage, Snapshot = snap, Stars = stars, Reward = reward };
    }
    /// <summary>
    /// 스테이지 조건을 평가해서 획득 별(0~3)을 반환.
    /// </summary>
    public int EvaluateStars(in NormalStageData stage, in StageEndSnapshot snap)
    {
        if (stage.Condition == null || stage.Condition.Count == 0) return 0;

        int met = 0;
        int maxStars = Mathf.Min(3, stage.Condition.Count); // 세이브 구조가 3성 고정이므로 3으로 캡
        for (int i = 0; i < maxStars; i++)
        {
            if (IsConditionMet(stage.Condition[i], snap))
                met++;
        }
        return met;
    }
    private void SelectStage(NormalStageData data)
    {
        _selectedStage = data;
    }

    private bool IsConditionMet(in Condition c, in StageEndSnapshot s)
    {
        switch (c.ClearType)
        {
            case ClearType.MoneySave: return s.moneyLeft >= c.Value;
            case ClearType.HealthSave: return s.baseHpRatio >= c.Value;   // 0~1
            case ClearType.UnitSave: return s.unitDestroyedCount < c.Value;
            default: return false;
        }
    }

    /// <summary>
    /// 스테이지 클리어 처리: 별 갱신, 보상 지급(젬은 3성 최초 1회), 저장까지 수행.
    /// </summary>
    public RewardResult ApplyClearAndSave(in NormalStageData stage, int starsEarned, bool giveGoldEveryClear = true)
    {
        RewardResult rewardResult = new RewardResult();

        // 안전 보정
        int maxPossible = stage.Condition != null ? stage.Condition.Count : 0;
        if (maxPossible > 3) maxPossible = 3;
        if (starsEarned < 0) starsEarned = 0;
        if (starsEarned > maxPossible) starsEarned = maxPossible;

        GameData gd = DataManager.Instance.GameData;
        if (gd == null)
        {
            Debug.LogError("[NormalStageManager] GameData가 없습니다. DataManager.LoadData() 확인");
            return rewardResult;
        }

        // ClearStage 찾기(없으면 생성)
        ClearStage cs = null;
        int idx = -1;
        if (gd.ClearStage == null) gd.ClearStage = new List<ClearStage>();
        for (int i = 0; i < gd.ClearStage.Count; i++)
        {
            if (gd.ClearStage[i].StageId == stage.Id)
            {
                cs = gd.ClearStage[i];
                idx = i;
                break;
            }
        }
        if (cs == null)
        {
            cs = new ClearStage
            {
                StageId = stage.Id,
                MaxStarNum = 0,
                Star = new Star()
            };
            gd.ClearStage.Add(cs);
            idx = gd.ClearStage.Count - 1;
        }

        // 이전/이후 최고 별
        int prevBest = cs.MaxStarNum;
        int newBest = Mathf.Max(prevBest, starsEarned);

        // 별 플래그 갱신(최고 기록 기준)
        cs.MaxStarNum = newBest;
        cs.Star.FirstStar = newBest >= 1;
        cs.Star.SecondStar = newBest >= 2;
        cs.Star.ThirdStar = newBest >= 3;

        // 골드 지급: 매번(원하면 첫 클리어만 지급으로 바꿔도 됨)
        if (giveGoldEveryClear)
        {
            gd.PlayerMoney += stage.Gold;
            rewardResult.GoldGained = stage.Gold;
        }

        // 젬 지급: 3성 '최초' 달성시에만 1회
        bool justHitThree = (prevBest < 3) && (newBest >= 3);
        if (justHitThree)
        {
            gd.PlayerGem += stage.Gem;
            rewardResult.GemGained = stage.Gem;
            rewardResult.FirstTimeThreeStar = true;
        }

        // 반영/저장
        gd.ClearStage[idx] = cs;
        DataManager.Instance.SaveData();

        // 이벤트 통지
        Action ev = OnStageProgressChanged;
        if (ev != null) ev.Invoke();

        rewardResult.NewBestStars = newBest;
        return rewardResult;
    }

    /// <summary>스테이지 세이브 조회. 없으면 null 반환.</summary>
    public ClearStage GetClearStageOrNull(string stageId)
    {
        GameData gd = DataManager.Instance.GameData;
        if (gd == null || gd.ClearStage == null) return null;

        for (int i = 0; i < gd.ClearStage.Count; i++)
            if (gd.ClearStage[i].StageId == stageId)
                return gd.ClearStage[i];

        return null;
    }

    /// <summary>스테이지 데이터 검색(월드들 안에서 id로). 찾으면 true.</summary>
    public bool TryFindStageById(string stageId, out NormalStageData stage, out WorldStageData world)
    {
        stage = default;
        world = default;
        List<WorldStageData> worlds = DataManager.Instance.WorldStageDatas;
        if (worlds == null) return false;

        for (int w = 0; w < worlds.Count; w++)
        {
            WorldStageData wd = worlds[w];
            if (wd.Stages == null) continue;

            for (int s = 0; s < wd.Stages.Count; s++)
            {
                NormalStageData st = wd.Stages[s];
                if (st.Id == stageId)
                {
                    stage = st;
                    world = wd;
                    return true;
                }
            }
        }
        return false;
    }
}

/// <summary>
/// 엔딩씬에서 판정에 필요한 값들 묶음(네 게임 쪽 값으로 채워서 넘겨줘)
/// </summary>
public struct StageEndSnapshot
{
    public int moneyLeft;           // 남은 돈
    public float baseHpRatio;       // 베이스 남은 체력 비율(0~1)
    public int unitDestroyedCount;  // 파괴된 유닛 수
}

/// <summary>보상/진척 결과</summary>
public struct RewardResult
{
    public int GoldGained;
    public int GemGained;
    public int NewBestStars;
    public bool FirstTimeThreeStar;
}
