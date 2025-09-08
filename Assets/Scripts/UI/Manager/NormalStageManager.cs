using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

public sealed class NormalStageManager : SimpleSingleton<NormalStageManager>
{
    private NormalStageData _selectedStage;                 // ¼±ÅÃµÈ ½ºÅ×ÀÌÁö µ¥ÀÌÅÍ
    private StageType _stageType = StageType.Stage1;

    public event Action OnStageProgressChanged;

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

    // ¢º Resources/RewardIcon/{ID}.png ·Îµå (Æú´õ/ÀÌ¸§ ¹Ýµå½Ã ÀÏÄ¡)
    private const string _rewardIconResourcesPath = "Reward";
    private readonly Dictionary<string, Sprite> _rewardIconMap =
        new Dictionary<string, Sprite>(StringComparer.OrdinalIgnoreCase);

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // »ý¼º/ÃÊ±âÈ­
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public NormalStageManager()
    {
        // ÀÌº¥Æ® ±¸µ¶
        EventManager.Instance.Subscribe<NormalStageData>("SelectStage", SelectStage);

        // ¾ÆÀÌÄÜ Ä³½Ã ¹Ì¸® ÀûÀç(¼±ÅÃ »çÇ×ÀÌÁö¸¸ ±ÇÀå)
        PrimeRewardIconCache();
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

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Å¬¸®¾î/º¸»ó Ã³¸®
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    /// <summary>½ºÅ×ÀÌÁö Å¬¸®¾î Ã³¸®: º° °»½Å, º¸»ó Áö±Þ(ÁªÀº 3¼º ÃÖÃÊ 1È¸), ÀúÀå±îÁö ¼öÇà.</summary>
    public RewardResult ApplyClearAndSave(in NormalStageData stage, int starsEarned, bool giveGoldEveryClear = true)
    {
        RewardResult rewardResult = new RewardResult();

        int maxPossible = stage.Condition != null ? stage.Condition.Count : 0;
        if (maxPossible > 3) maxPossible = 3;
        if (starsEarned < 0) starsEarned = 0;
        if (starsEarned > maxPossible) starsEarned = maxPossible;

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

        Action ev = OnStageProgressChanged;
        if (ev != null) ev.Invoke();

        rewardResult.NewBestStars = newBest;
        return rewardResult;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Á¶È¸/°Ë»ö
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    /// <summary>½ºÅ×ÀÌÁö ¼¼ÀÌºê Á¶È¸. ¾øÀ¸¸é null.</summary>
    public ClearStage GetClearStageOrNull(string stageId)
    {
        GameData gd = DataManager.Instance.GameData;
        if (gd == null || gd.ClearStage == null) return null;

        for (int i = 0; i < gd.ClearStage.Count; i++)
            if (gd.ClearStage[i].StageId == stageId)
                return gd.ClearStage[i];
        return null;
    }

    /// <summary>¿ùµåµé¿¡¼­ id·Î ½ºÅ×ÀÌÁö °Ë»ö.</summary>
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

    /// <summary>
    /// id·Î ½ºÅ×ÀÌÁö Á¶È¸. ¾øÀ¸¸é default ¹ÝÈ¯(SelectedStage°¡ µ¿ÀÏ idÀÌ¸é ±×°É ¹ÝÈ¯).
    /// </summary>
    public NormalStageData GetStageOrDefault(string stageId)
    {
        NormalStageData found;
        WorldStageData dummyWorld;
        if (TryFindStageById(stageId, out found, out dummyWorld)) return found;

        if (!string.IsNullOrEmpty(_selectedStage.Id) && _selectedStage.Id == stageId)
            return _selectedStage;

        return default;
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

    /// <summary>
    /// ½ºÅ×ÀÌÁö º¸»ó ¸ñ·ÏÀ» (id, value) ¸®½ºÆ®·Î ¹ÝÈ¯.
    /// stageId¸¦ ¸ø Ã£À¸¸é SelectedStage ±âÁØÀ¸·Î ¹é¾÷.
    /// </summary>
    public List<(string id, int value)> GetRewardsForStage(string stageId)
    {
        List<(string id, int value)> list = new List<(string id, int value)>();

        NormalStageData s = GetStageOrDefault(stageId);
        if (string.IsNullOrEmpty(s.Id))
        {
            // fallback: ¼±ÅÃµÈ ½ºÅ×ÀÌÁö »ç¿ë
            s = _selectedStage;
        }

        if (!string.IsNullOrEmpty(s.Id))
        {
            if (s.Gold > 0) list.Add(("Gold", s.Gold));
            if (s.Gem > 0) list.Add(("Gem", s.Gem));
            // TODO: µ¿Àû º¸»ó Å×ÀÌºí Ãß°¡ ½Ã ¿©±â¼­ º´ÇÕ
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
