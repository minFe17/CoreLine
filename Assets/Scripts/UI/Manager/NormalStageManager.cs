using System;
using System.Collections.Generic;
using UnityEngine;
using Utils; // SimpleSingleton

// 스테이지 종료 시 넘겨줄 스냅샷(엔딩씬/게임매니저가 채워서 Invoke)
[Serializable]
public struct StageEndSnapshot
{
    public string StageId;  // 어떤 스테이지를 끝냈는지
    public int Money;       // 남긴(또는 획득) 돈
    public int Health;      // 보통 King/기지 HP(또는 %*100)
    public int DestroyedUnit;   // 파괴된 유닛 수
}

// 클리어 결과(이벤트로 내보내 UI/보상 처리)

//클리어 스테이지(리스트+구조체)
//스테이지 ID,맥스스타넘(젬 조건), Star(구조체)(트루면 조건달성 아니면 미달성)
[Serializable]
public struct StageClearResult
{
    public string StageId;
    public bool Cleared;
    public List<Condition> Star; // 성공한 조건
    public int GoldReward;
    public int GemReward;
    public string UnlockCharacter;
}

public class NormalStageManager : SimpleSingleton<NormalStageManager>
{
    private readonly Dictionary<string, NormalStageData> _stagesById = new Dictionary<string, NormalStageData>();
    private NormalStageData _selectedStage;
    private bool _hasSelected;

    private readonly HashSet<string> _clearedStages = new HashSet<string>();
    private const string ClearedKey = "normal_stage_cleared_ids";

    public NormalStageManager()
    {
        LoadStagesFromDataManager();
        LoadClearedFromPrefs();

        EventManager.Instance.Subscribe<string>("SelectNormalStage", OnSelectStage);
        EventManager.Instance.Subscribe<StageEndSnapshot>("EndNormalStage", OnEndStage);
    }

    // ───────────────────────────────────────────────────────────
    // 로드/저장
    // ───────────────────────────────────────────────────────────
    private void LoadStagesFromDataManager()
    {
        _stagesById.Clear();
        // DataManager에 NormalStageDatas가 있다고 가정
        List<NormalStageData> list = DataManager.Instance.NormalStageDatas;
        if (list == null)
        {
            Debug.LogWarning("[NormalStageManager] NormalStageDatas is null.");
            return;
        }

        for (int i = 0; i < list.Count; i++)
        {
            NormalStageData s = list[i];
            if (string.IsNullOrEmpty(s.Id))
            {
                Debug.LogWarning($"[NormalStageManager] Stage[{i}] has empty Id. Skipped.");
                continue;
            }
            _stagesById[s.Id] = s;
        }
    }

    private void LoadClearedFromPrefs()
    {
        _clearedStages.Clear();
        if (!PlayerPrefs.HasKey(ClearedKey)) return;

        string csv = PlayerPrefs.GetString(ClearedKey, "");
        if (string.IsNullOrEmpty(csv)) return;

        string[] ids = csv.Split(',');
        for (int i = 0; i < ids.Length; i++)
        {
            string id = ids[i].Trim();
            if (!string.IsNullOrEmpty(id)) _clearedStages.Add(id);
        }
    }

    private void SaveClearedToPrefs()
    {
    }

    // ───────────────────────────────────────────────────────────
    // 공개 API
    // ───────────────────────────────────────────────────────────
    public bool TryGetStage(string id, out NormalStageData data) => _stagesById.TryGetValue(id, out data);
    public List<NormalStageData> GetAllStages() => new List<NormalStageData>(_stagesById.Values);
    public bool HasSelected => _hasSelected;
    public NormalStageData SelectedStage => _selectedStage;
    public bool IsCleared(string id) => _clearedStages.Contains(id);

    public void SelectStage(string id)
    {
        if (!_stagesById.TryGetValue(id, out _selectedStage))
        {
            _hasSelected = false;
            Debug.LogWarning($"[NormalStageManager] Stage '{id}' not found.");
            return;
        }
        _hasSelected = true;

        // 선택 알림(필요 시 UI가 구독)
        EventManager.Instance.Invoke<NormalStageData>("NormalStageSelected", _selectedStage);
    }

    /// 조건 텍스트(Info 비었을 때 기본 생성)
    public static string BuildConditionText(in Condition c)
    {
        if (!string.IsNullOrEmpty(c.Info)) return c.Info;

        switch (c.ClearType)
        {
            case ClearType.MoneySave: return $"자원 {c.Value} 이상 보유";
            case ClearType.HealthSave: return $"기지 체력 {c.Value} 이상 유지";
            case ClearType.UnitSave: return $"유닛 {c.Value}기 이상 생존";
        }
        return $"조건: {c.ClearType} {c.Value}";
    }

    // ───────────────────────────────────────────────────────────
    // 이벤트 핸들러 (EventManager에서 호출)
    // ───────────────────────────────────────────────────────────
    private void OnSelectStage(string id)
    {
        SelectStage(id);
    }

    private void OnEndStage(StageEndSnapshot snap)
    {
        if (!_stagesById.TryGetValue(snap.StageId, out NormalStageData stage))
        {
            Debug.LogWarning($"[NormalStageManager] EndStage for unknown id={snap.StageId}");
            return;
        }

        StageClearResult result = Evaluate(stage, snap);

        // 첫 클리어 기록
        if (result.Cleared && !_clearedStages.Contains(stage.Id))
        {
            _clearedStages.Add(stage.Id);
            SaveClearedToPrefs();
        }

        //보상 지급
        if (result.Cleared)
        {
            if (stage.Gold > 0) EventManager.Instance.Invoke<int>("GrantMetaGold", stage.Gold);
            if (stage.Gem > 0 && result.Star.Count == 3) EventManager.Instance.Invoke<int>("GrantMetaGem", stage.Gem);
            if (!string.IsNullOrEmpty(stage.UnlockCharacter))
                EventManager.Instance.Invoke<string>("UnlockCharacter", stage.UnlockCharacter);
        }

        // 결과 브로드캐스트 (엔딩 UI 등)
        EventManager.Instance.Invoke<StageClearResult>("NormalStageClearResult", result);
    }

    // ───────────────────────────────────────────────────────────
    // 판정 로직
    // ───────────────────────────────────────────────────────────
    public StageClearResult Evaluate(NormalStageData stage, in StageEndSnapshot snap)
    {
        List<Condition> star = new List<Condition>();
        if (stage.Condition != null)
        {
            for (int i = 0; i < stage.Condition.Count; i++)
            {
                Condition c = stage.Condition[i];
                if (IsConditionMet(c, snap))
                    star.Add(c);
            }
        }

        StageClearResult result = new StageClearResult
        {
            StageId = stage.Id,
            Cleared = star.Count == 0,
            Star = star,
            GoldReward = stage.Gold,
            GemReward = stage.Gem,
            UnlockCharacter = stage.UnlockCharacter
        };
        return result;
    }

    private static bool IsConditionMet(in Condition c, in StageEndSnapshot endResult)
    {
        float v = c.Value;

        switch (c.ClearType)
        {
            case ClearType.MoneySave: return endResult.Money >= v;
            case ClearType.HealthSave: return endResult.Health >= v;
            case ClearType.UnitSave: return endResult.DestroyedUnit < v;
        }
        return true; // 정의 안 된 타입은 통과로 처리
    }
}
