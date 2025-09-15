using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public sealed class NormalStageManager : SimpleSingleton<NormalStageManager>
{
    private NormalStageData _selectedStage;                 // ¼±ÅÃµÈ ½ºÅ×ÀÌÁö µ¥ÀÌÅÍ
    private StageType _stageType = StageType.Stage1;

    // ÁøÇàµµ ÀúÀåÀÌ ¹Ù²ð ¶§(¼¼ÀÌºê ¹Ý¿µ µî)
    public event Action OnStageProgressChanged;

    // ¡Ú Å¬¸®¾î½Ã UI°¡ ¹ÞÀ» ÀÌº¥Æ®(°ÔÀÓ¸Å´ÏÀú°¡ ±¸µ¶)
    public event Action<NormalStageData, StageEndSnapshot, int, RewardResult> StageCleared;
    public event Action<NormalStageData, StageEndSnapshot, int, RewardResult> StageDefeated;
    public struct LastRun
    {
        public NormalStageData Stage;
        public StageEndSnapshot Snapshot;
        public int Stars;
        public RewardResult Reward;
    }

    public StageType StageType
    {
        get { return _stageType; }
        set
        {
            _stageType = value;
            EventManager.Instance.Invoke<StageType>("ChangeStage", _stageType); // »ö º¯°æ Àü¿ë
        }
    }

    private LastRun? _lastResult;
    public LastRun? LastResult { get { return _lastResult; } }

    public NormalStageData SelectedStage { get { return _selectedStage; } }

    // ¢º Resources/Reward/{ID}.png ·Îµå (Æú´õ/ÀÌ¸§ ¹Ýµå½Ã ÀÏÄ¡)
    private const string _rewardIconResourcesPath = "Reward";
    private readonly Dictionary<string, Sprite> _rewardIconMap =
        new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // »ý¼º/ÃÊ±âÈ­
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public NormalStageManager()
    {
        // ¼±ÅÃ ÀÌº¥Æ® ±¸µ¶
        EventManager.Instance.Subscribe<NormalStageData>("SelectStage", SelectStage);

        // ¾ÆÀÌÄÜ Ä³½Ã
        PrimeRewardIconCache();
    }

    private void SelectStage(NormalStageData data)
    {
        _selectedStage = data;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // °á°ú/Á¶°Ç Æò°¡
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void SetLastResult(in NormalStageData stage, in StageEndSnapshot snap, int stars, in RewardResult reward)
    {
        _lastResult = new LastRun { Stage = stage, Snapshot = snap, Stars = stars, Reward = reward };
    }

    /// <summary>½ºÅ×ÀÌÁö Á¶°ÇÀ» Æò°¡ÇØ¼­ È¹µæ º°(0~3)À» ¹ÝÈ¯.</summary>
    public int EvaluateStars(in NormalStageData stage, in StageEndSnapshot snap)
    {
        if (stage.Condition == null || stage.Condition.Count == 0) return 0;

        int maxStars = Mathf.Min(3, stage.Condition.Count);
        int met = 0;
        for (int i = 0; i < maxStars; i++)
        {
            if (IsConditionMet(stage.Condition[i], snap)) met++;
        }
        return met;
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

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¡Ú Å¬¸®¾î/ÆÐ¹è Ã³¸® ¡æ ÀÌº¥Æ® ¹ßÇà
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    /// <summary>¼º°ø Å¬¸®¾î: º° °è»ê, º¸»ó Áö±Þ/ÀúÀå, ÀÌº¥Æ® ¹ßÇà.</summary>
    public void CompleteStageSuccess(in StageEndSnapshot snap)
    {
        MonoSingleton<AudioClipManager>.Instance.StopBGM();

        // ÇöÀç ¼±ÅÃµÈ ½ºÅ×ÀÌÁö ±âÁØ
        NormalStageData stage = _selectedStage;
        if (string.IsNullOrEmpty(stage.Id))
        {
            Debug.LogWarning("[NormalStageManager] SelectedStage °¡ ºñ¾îÀÖ½À´Ï´Ù. SelectStage ÀÌº¥Æ®·Î ¼±ÅÃ ¸ÕÀú ÇØÁÖ¼¼¿ä.");
            return;
        }

        // º° °è»ê + º¸»ó ¹Ý¿µ/¼¼ÀÌºê
        int stars = EvaluateStars(stage, snap);
        var reward = ApplyClearAndSave(stage, stars, giveGoldEveryClear: true);

        // ¸¶Áö¸· °á°ú Ä³½Ì
        SetLastResult(stage, snap, stars, reward);

        // ÀÌº¥Æ® ¹ßÇà ¡æ GameManager.OnStageCleared(...) È£ÃâµÊ
        var ev = StageCleared;
        if (ev != null) ev.Invoke(stage, snap, stars, reward);
    }

    /// <summary>ÆÐ¹è Ã³¸®(¿øÇÏ¸é º°=0, º¸»ó ¾øÀ½À¸·Î ¹ßÇà).</summary>
    public void CompleteStageDefeat(in StageEndSnapshot snap)
    {
        MonoSingleton<AudioClipManager>.Instance.StopBGM();

        NormalStageData stage = _selectedStage;
        if (string.IsNullOrEmpty(stage.Id)) return;

        int stars = 0;
        var reward = new RewardResult(); // ÆÐ¹è º¸»ó ¾øÀ½
        SetLastResult(stage, snap, stars, reward);

        var ev = StageDefeated;           
        if (ev != null) ev.Invoke(stage, snap, stars, reward);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Å¬¸®¾î/º¸»ó Ã³¸®(¼¼ÀÌºê)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    /// <summary>½ºÅ×ÀÌÁö Å¬¸®¾î Ã³¸®: º° °»½Å, º¸»ó Áö±Þ(ÁªÀº 3¼º ÃÖÃÊ 1È¸), ÀúÀå±îÁö ¼öÇà.</summary>
    public RewardResult ApplyClearAndSave(in NormalStageData stage, int starsEarned, bool giveGoldEveryClear = true)
    {
        RewardResult rewardResult = new RewardResult();

        int maxPossible = stage.Condition != null ? stage.Condition.Count : 0;
        if (maxPossible > 3) maxPossible = 3;
        starsEarned = Mathf.Clamp(starsEarned, 0, maxPossible);

        GameData gd = DataManager.Instance.GameData;
        if (gd == null)
        {
            Debug.LogError("[NormalStageManager] GameData°¡ ¾ø½À´Ï´Ù. DataManager.LoadData() È®ÀÎ");
            return rewardResult;
        }

        // ClearStage Ã£±â(¾øÀ¸¸é »ý¼º)
        if (gd.ClearStage == null) gd.ClearStage = new List<ClearStage>();
        ClearStage cs = null;
        int idx = -1;
        for (int i = 0; i < gd.ClearStage.Count; i++)
        {
            if (gd.ClearStage[i].StageId == stage.Id) { cs = gd.ClearStage[i]; idx = i; break; }
        }
        if (cs == null)
        {
            cs = new ClearStage { StageId = stage.Id, MaxStarNum = 0, Star = new Star() };
            gd.ClearStage.Add(cs);
            idx = gd.ClearStage.Count - 1;
        }

        int prevBest = cs.MaxStarNum;
        int newBest = Mathf.Max(prevBest, starsEarned);

        cs.MaxStarNum = newBest;
        cs.Star.FirstStar = newBest >= 1;
        cs.Star.SecondStar = newBest >= 2;
        cs.Star.ThirdStar = newBest >= 3;

        // °ñµå: ±âº» ¸Å Å¬¸®¾î Áö±Þ
        if (giveGoldEveryClear && stage.Gold > 0)
        {
            gd.PlayerMoney += stage.Gold;
            rewardResult.GoldGained = stage.Gold;
        }

        // Áª: 3¼º ÃÖÃÊ ´Þ¼º 1È¸¸¸
        bool justHitThree = (prevBest < 3) && (newBest >= 3);
        if (justHitThree && stage.Gem > 0)
        {
            gd.PlayerGem += stage.Gem;
            rewardResult.GemGained = stage.Gem;
            rewardResult.FirstTimeThreeStar = true;
        }

        gd.ClearStage[idx] = cs;
        DataManager.Instance.SaveData();

        OnStageProgressChanged?.Invoke();
        rewardResult.NewBestStars = newBest;
        return rewardResult;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Á¶È¸/°Ë»ö
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public ClearStage GetClearStageOrNull(string stageId)
    {
        GameData gd = DataManager.Instance.GameData;
        if (gd == null || gd.ClearStage == null) return null;

        for (int i = 0; i < gd.ClearStage.Count; i++)
            if (gd.ClearStage[i].StageId == stageId)
                return gd.ClearStage[i];
        return null;
    }

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

    public NormalStageData GetStageOrDefault(string stageId)
    {
        NormalStageData found;
        WorldStageData _;
        if (TryFindStageById(stageId, out found, out _)) return found;

        if (!string.IsNullOrEmpty(_selectedStage.Id) && _selectedStage.Id == stageId)
            return _selectedStage;

        return default;
    }

    /// <summary>ÇöÀç ¼±ÅÃµÈ ½ºÅ×ÀÌÁö ±âÁØÀ¸·Î ¡°´ÙÀ½ ½ºÅ×ÀÌÁö¡± Á¶È¸.</summary>
    public bool TryGetNextStageFromSelected(out NormalStageData next, out WorldStageData world)
    {
        next = default; world = default;
        if (string.IsNullOrEmpty(_selectedStage.Id)) return false;

        List<WorldStageData> worlds = DataManager.Instance.WorldStageDatas;
        if (worlds == null) return false;

        for (int w = 0; w < worlds.Count; w++)
        {
            var wd = worlds[w];
            var list = wd.Stages;
            if (list == null || list.Count == 0) continue;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Id == _selectedStage.Id)
                {
                    if (i + 1 < list.Count)
                    {
                        next = list[i + 1]; world = wd; return true;        // °°Àº ¿ùµå ´ÙÀ½
                    }
                    // ¸¶Áö¸·ÀÌ¸é ´ÙÀ½ ¿ùµå Ã¹ ½ºÅ×ÀÌÁö
                    if (w + 1 < worlds.Count && worlds[w + 1].Stages != null && worlds[w + 1].Stages.Count > 0)
                    {
                        world = worlds[w + 1];
                        next = world.Stages[0];
                        return true;
                    }
                    return false;
                }
            }
        }
        return false;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¸®¿öµå ¾ÆÀÌÄÜ
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void PrimeRewardIconCache()
    {
        Sprite[] all = Resources.LoadAll<Sprite>(_rewardIconResourcesPath);
        for (int i = 0; i < all.Length; i++)
        {
            Sprite s = all[i];
            if (s != null && !_rewardIconMap.ContainsKey(s.name))
                _rewardIconMap.Add(s.name, s);
        }
    }

    public void RegisterRewardIcon(string id, Sprite sprite)
    {
        if (string.IsNullOrEmpty(id) || sprite == null) return;
        _rewardIconMap[id] = sprite;
    }

    public Sprite GetRewardIcon(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        Sprite s;
        if (_rewardIconMap.TryGetValue(id, out s)) return s;

        s = Resources.Load<Sprite>(_rewardIconResourcesPath + "/" + id);
        if (s != null) _rewardIconMap[id] = s;
        return s;
    }

    public List<(string id, int value)> GetRewardsForStage(string stageId)
    {
        List<(string id, int value)> list = new List<(string id, int value)>();
        NormalStageData s = GetStageOrDefault(stageId);
        if (string.IsNullOrEmpty(s.Id)) s = _selectedStage;

        if (!string.IsNullOrEmpty(s.Id))
        {
            if (s.Gold > 0) list.Add(("Gold", s.Gold));
            if (s.Gem > 0) list.Add(("Gem", s.Gem));
        }
        return list;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¿£µù¾À ½º³À¼¦/º¸»ó °á°ú
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public struct StageEndSnapshot
    {
        public int moneyLeft;          // ³²Àº µ·
        public float baseHpRatio;        // º£ÀÌ½º ³²Àº Ã¼·Â ºñÀ²(0~1)
        public int unitDestroyedCount; // ÆÄ±«µÈ À¯´Ö ¼ö
    }

    public struct RewardResult
    {
        public int GoldGained;
        public int GemGained;
        public int NewBestStars;
        public bool FirstTimeThreeStar;
    }
}
