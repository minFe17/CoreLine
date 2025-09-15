using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

/// <summary>
/// ½ºÅ³Àº ·Îµå¾Æ¿ô¿£ Id¸¸ ÀúÀåÇÏ°í, ½ÇÁ¦ ¼öÄ¡(Cost/Value/Cooltime/Duration)´Â
/// CSV(DataManager.SkillDatas)¿¡¼­ ·±Å¸ÀÓ¿¡ Á¶È¸ÇØ »ç¿ëÇÏ´Â ±¸Á¶.
/// </summary>
public class SkillManager : MonoSingleton<SkillManager>
{
    // ===== CSV Á¤ÀÇ Ä³½Ã =====
    public struct SkillDef
    {
        public string Id;
        public int Cost;
        public float Value;
        public float Cooltime;
        public float Duration;
    }

    // ·Îµå¾Æ¿ô ¿£Æ®¸®: Id¸¸ ÀúÀå
    public struct SelectedSkill
    {
        public string Id;

        public SelectedSkill(string id) => Id = id;

        // ÇÚµé·¯°¡ °£ÆíÈ÷ CSV Á¤ÀÇ¸¦ °¡Á®°¡µµ·Ï Á¦°ø
        public bool TryGetDef(out SkillDef def) => SkillManager.Instance.TryGetDef(Id, out def);
    }

    // ÇÁ¸®ºä Å¸°ÙÆÃ ±¸ºÐ(±âÁ¸ À¯Áö)
    [System.Flags]
    public enum TargetKind
    {
        Towers = 1 << 0,
        Monsters = 1 << 1,
        Both = Towers | Monsters
    }

    [Header("Selection/Slots")]
    [SerializeField] private int _maxSlots = 3;
    public readonly List<SelectedSkill> _loadout = new List<SelectedSkill>();

    [Header("Optional tags/roots")]
    [SerializeField] private Transform _unitRoot;     // ¼±ÅÃ
    [SerializeField] private string _monsterTag = "Monster";

    // ÇÚµé·¯(¾ÆÀÌµð ¡æ ±¸Çö)
    private readonly Dictionary<string, ITowerSkillHandler> _towerHandlers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IMonsterSkillHandler> _monsterHandlers = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<TargetType, IIncomeSkillHandler> _incomeHandlers = new();
    private readonly Dictionary<string, ISkillTargetingSpecProvider> _targetingProviders = new(StringComparer.OrdinalIgnoreCase);

    // CSV Ä³½Ã
    private readonly Dictionary<string, SkillDef> _defs = new(StringComparer.OrdinalIgnoreCase);

    // ½½·Ôº° Äð´Ù¿î Á¾·á ½Ã°¢
    private readonly Dictionary<int, float> _slotCooldownEnd = new();

    public event Action OnLoadoutChanged;

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÃÊ±âÈ­: CSV ÀÐ¾î Ä³½Ã, ºôÆ®ÀÎ ÇÚµé·¯ µî·Ï
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void Awake()
    {
        // CSV ¡æ ·±Å¸ÀÓ Ä³½Ã
        _defs.Clear();
        var rows = DataManager.Instance?.SkillDatas;
        if (rows != null)
        {
            for (int i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                _defs[r.Id] = new SkillDef
                {
                    Id = r.Id,
                    Cost = r.Cost,
                    Value = r.Value,
                    Cooltime = r.Cooltime,
                    Duration = r.Duration
                };
            }
        }

        RegisterBuiltinSkills();
    }

    // ÇÁ·ÎÁ§Æ®¿¡ ÀÖ´Â ±¸Çö¿¡ ¸ÂÃç µî·Ï(¾ø´Â °æ¿ì ÁÖ¼®Ã³¸®ÇØµµ µÊ)
    private void RegisterBuiltinSkills()
    {
        // ¿¹½Ã: Å¸¿ö(¾Æ±º) Èú
        if (TryMake<RangeHeal>(out var rh)) { RegisterTowerHandler(rh); RegisterTargetingProvider(rh); }

        // ¿¹½Ã: ¸ó½ºÅÍ °ø°Ý/µð¹öÇÁ
        if (TryMake<ArrowRain>(out var ar)) { RegisterMonsterHandler(ar); RegisterTargetingProvider(ar); }
        if (TryMake<MonsterSlow>(out var ms)) { RegisterMonsterHandler(ms); RegisterTargetingProvider(ms); }

        // ÀÎÄÄ ½ºÅ³ »ç¿ë ½Ã
        if (TryMake<IncomeMoneyHandler>(out var im)) RegisterIncomeHandler(im);
        if (TryMake<IncomeSkillHandler>(out var iskill)) RegisterIncomeHandler(iskill);
    }

    private static bool TryMake<T>(out T obj) where T : class, new()
    {
        try { obj = new T(); return true; } catch { obj = null; return false; }
    }

    private void RegisterTowerHandler(ITowerSkillHandler h) { if (h != null && !string.IsNullOrEmpty(h.Id)) _towerHandlers[h.Id] = h; }
    private void RegisterMonsterHandler(IMonsterSkillHandler h) { if (h != null && !string.IsNullOrEmpty(h.Id)) _monsterHandlers[h.Id] = h; }
    private void RegisterIncomeHandler(IIncomeSkillHandler h) { if (h != null) _incomeHandlers[h.TargetType] = h; }
    private void RegisterTargetingProvider(ISkillTargetingSpecProvider p)
    {
        if (p != null && !string.IsNullOrEmpty(p.Id)) _targetingProviders[p.Id] = p;
    }

    // CSV Á¤ÀÇ Á¶È¸(¿ÜºÎ/ÇÚµé·¯/ÇÁ¸®ºä¿¡¼­ »ç¿ë)
    public bool TryGetDef(string id, out SkillDef def) => _defs.TryGetValue(id, out def);

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ·Îµå¾Æ¿ô API
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public bool AddToLoadout(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;
        if (_loadout.Count >= _maxSlots) return false;
        if (_loadout.Exists(s => s.Id == id)) return false; // Áßº¹ ¹æÁö
        if (!_defs.ContainsKey(id)) { Debug.LogWarning($"[SkillManager] CSV¿¡ ¾ø´Â ½ºÅ³: {id}"); return false; }

        _loadout.Add(new SelectedSkill { Id = id });
        OnLoadoutChanged?.Invoke();
        return true;
    }

    // (±¸¹öÀü È£È¯) ±âÁ¸ LaboratoryData·Î µé¾î¿À¸é Id¸¸ ÃëÇÔ
    public bool AddToLoadout(in LaboratoryData def)
    => !string.IsNullOrEmpty(def.Id) && AddToLoadout(def.Id);

    public bool RemoveFromLoadout(in LaboratoryData def)
    => !string.IsNullOrEmpty(def.Id) && RemoveFromLoadout(def.Id);

    // 2) SelectedSkillÀ» Á÷Á¢ ³Ñ±â´Â °æ¿ìµµ Áö¿ø
    public bool RemoveFromLoadout(in SelectedSkill sel)
        => !string.IsNullOrEmpty(sel.Id) && RemoveFromLoadout(sel.Id);
    public bool RemoveFromLoadout(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;

        int removed = _loadout.RemoveAll(s =>
            string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));

        if (removed > 0)
        {
            OnLoadoutChanged?.Invoke();
            return true;
        }
        return false;
    }
    public void ClearLoadout()
    {
        if (_loadout.Count == 0) return;
        _loadout.Clear();
        OnLoadoutChanged?.Invoke();
    }

    public SelectedSkill GetSelectedSkillBySlotIndex(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _loadout.Count) throw new IndexOutOfRangeException();
        return _loadout[slotIndex];
    }

    public string[] GetLoadoutIds()
    {
        var arr = new string[_loadout.Count];
        for (int i = 0; i < _loadout.Count; i++) arr[i] = _loadout[i].Id;
        return arr;
    }

    public bool TryGetTargetingSpec(in SelectedSkill skill, out SkillTargetingSpec spec)
    {
        if (_targetingProviders.TryGetValue(skill.Id, out var provider))
        {
            spec = provider.GetSpec(in skill);
            return true;
        }

        // ±âº»(Æ÷ÀÎÆ®, ¾Æ¹« ÀÇ¹Ì ¾ø´Â µðÆúÆ®)
        spec = new SkillTargetingSpec
        {
            Mode = TargetingMode.Point,
            HalfSizeCells = 0,
            RadiusWorld = 0f,
            ValidTargets = TargetKind.Both
        };
        return false;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ½ºÅ³ »ç¿ë(Æ÷ÀÎÆ®/»ç°¢/¿øÇü)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void UseSkill(int slotIndex, GameObject explicitTarget = null)
    {
        if (PauseControl.IsPaused) return;
        if (!ValidateSlotAndPay(slotIndex, out var sel, out var def)) return;

        // ÀÎÄÄ ½ºÅ³ÀÌ¸é Áï½Ã º¸»ó Àû¿ë
        if (TryApplyIncomeReward(in sel)) return;

        // Å¸°Ù ¿ÀºêÁ§Æ®°¡ ÁÖ¾îÁ³À» ¶§¸¸ ´ÜÀÏ Àû¿ë
        if (explicitTarget != null) DispatchToTarget(explicitTarget, in sel);
    }

    public void UseSkillAreaRectWorld(int slotIndex, Vector3 centerWorld, int halfSizeCells, TargetKind targetKind)
    {
        if (PauseControl.IsPaused) return;
        if (!ValidateSlotAndPay(slotIndex, out var sel, out var def)) return;
        if (TryApplyIncomeReward(in sel)) return;

        centerWorld.z = 0f;
        var targets = AcquireTargetsRectWorld(centerWorld, halfSizeCells, targetKind);
        ApplyEffectToTargets(in sel, targets);
    }

    public void UseSkillAreaRadiusWorld(int slotIndex, Vector3 centerWorld, float radiusWorld, TargetKind targetKind)
    {
        if (PauseControl.IsPaused) return;
        if (!ValidateSlotAndPay(slotIndex, out var sel, out var def)) return;
        if (TryApplyIncomeReward(in sel)) return;

        centerWorld.z = 0f;
        var targets = AcquireTargetsRadiusWorld(centerWorld, radiusWorld, targetKind);
        ApplyEffectToTargets(in sel, targets);
    }

    // ºñ¿ë/Äð´Ù¿î °øÅë °ËÁõ
    private bool ValidateSlotAndPay(int slotIndex, out SelectedSkill sel, out SkillDef def)
    {
        sel = default; def = default;

        if (slotIndex < 0 || slotIndex >= _loadout.Count) return false;

        // Äð´Ù¿î Ã¼Å©
        if (_slotCooldownEnd.TryGetValue(slotIndex, out var end) && Time.time < end)
            return false;

        sel = _loadout[slotIndex];

        if (!TryGetDef(sel.Id, out def)) { Debug.LogWarning($"[SkillManager] Á¤ÀÇ ¾øÀ½: {sel.Id}"); return false; }

        // ºñ¿ë Â÷°¨
        if (!CostManager.Instance || !CostManager.Instance.TrySpendSkill(def.Cost))
        {
            Debug.LogWarning("[SkillManager] ½ºÅ³ ÄÚ½ºÆ® ºÎÁ·");
            return false;
        }

        // Äð´Ù¿î ½ÃÀÛ
        _slotCooldownEnd[slotIndex] = Time.time + Mathf.Max(0f, def.Cooltime);
        return true;
    }

    // ÀÎÄÄ ½ºÅ³ Ã³¸® (ÇÊ¿ä ¾øÀ¸¸é Ç×»ó false ¹ÝÈ¯)
    private bool TryApplyIncomeReward(in SelectedSkill selectedSkill)
    {
        // Effect.TargetTypeÀ¸·Î ºÐ±âÇÏ´ø °ú°Å ±¸Á¶¸¦ ¾²Áö ¾ÊÀ¸¹Ç·Î
        // ÀÎÄÄ ½ºÅ³À» ¾²½Ç °Å¸é Idº°·Î ÇÚµé·¯¸¦ µî·ÏÇØ »ç¿ëÇÏ¼¼¿ä.
        // ¾Æ·¡´Â ¿¹½Ã(ÇÚµé·¯°¡ µî·ÏµÇ¾î ÀÖÀ¸¸é ±×ÂÊ¿¡¼­ Ã³¸®).
        return false;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ½ÇÁ¦ Àû¿ë(´ÜÀÏ/º¹¼ö)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void DispatchToTarget(GameObject target, in SelectedSkill sel)
    {
        if (target == null) return;

        if (target.CompareTag(_monsterTag))
        {
            if (_monsterHandlers.TryGetValue(sel.Id, out var mh))
                mh.Apply(target, in sel);
            return;
        }

        var map = MapManager.Instance;
        if (map != null && map.IsReady)
        {
            var cell = map.WorldToCell(target.transform.position);
            if (map.TryGetTowerAt(cell, out var tower) && tower == target)
            {
                if (_towerHandlers.TryGetValue(sel.Id, out var th))
                    th.Apply(target, in sel);
            }
        }
    }

    private void ApplyEffectToTargets(in SelectedSkill sel, List<GameObject> targets)
    {
        if (targets == null || targets.Count == 0) return;

        for (int i = 0; i < targets.Count; i++)
        {
            var obj = targets[i];
            if (obj == null || !obj.activeInHierarchy) continue;

            if (obj.CompareTag(_monsterTag))
            {
                if (_monsterHandlers.TryGetValue(sel.Id, out var mh))
                    mh.Apply(obj, in sel);
            }
            else
            {
                if (_towerHandlers.TryGetValue(sel.Id, out var th))
                    th.Apply(obj, in sel);
            }
        }
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Å¸°Ù ¼öÁý(¸Ê ±â¹Ý)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private List<GameObject> AcquireTargetsRectWorld(Vector3 worldCenter, int halfSizeCells, TargetKind targetKind)
    {
        var map = MapManager.Instance;
        var result = new List<GameObject>(32);
        if (map == null || !map.IsReady) return result;

        Vector3 world2D = worldCenter; world2D.z = 0f;
        Vector3Int centerCell = map.WorldToCell(world2D);

        return AcquireTargetsRectCells(centerCell, halfSizeCells, targetKind);
    }

    private List<GameObject> AcquireTargetsRectCells(Vector3Int centerCell, int halfSizeCells, TargetKind targetKind)
    {
        var map = MapManager.Instance;
        var result = new List<GameObject>(32);
        if (map == null || !map.IsReady) return result;

        var uniq = new HashSet<GameObject>();
        BoundsInt bounds = map.GetNavBounds();

        int minX = Mathf.Max(bounds.xMin, centerCell.x - halfSizeCells);
        int maxX = Mathf.Min(bounds.xMax - 1, centerCell.x + halfSizeCells);
        int minY = Mathf.Max(bounds.yMin, centerCell.y - halfSizeCells);
        int maxY = Mathf.Min(bounds.yMax - 1, centerCell.y + halfSizeCells);

        if ((targetKind & TargetKind.Towers) != 0)
        {
            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                {
                    var cell = new Vector3Int(x, y, 0);
                    if (map.TryGetTowerAt(cell, out var tower) && tower != null && tower.activeInHierarchy)
                        if (uniq.Add(tower)) result.Add(tower);
                }
        }

        if ((targetKind & TargetKind.Monsters) != 0)
        {
            var monsters = GameObject.FindGameObjectsWithTag(_monsterTag);
            for (int i = 0; i < monsters.Length; i++)
            {
                var m = monsters[i];
                if (m == null || !m.activeInHierarchy) continue;
                var mc = map.WorldToCell(m.transform.position);
                if (mc.x < minX || mc.x > maxX || mc.y < minY || mc.y > maxY) continue;
                if (uniq.Add(m)) result.Add(m);
            }
        }

        return result;
    }

    private List<GameObject> AcquireTargetsRadiusWorld(Vector3 worldCenter, float radiusWorld, TargetKind targetKind)
    {
        var map = MapManager.Instance;
        var result = new List<GameObject>(32);
        if (map == null || !map.IsReady) return result;

        Vector3 c = worldCenter; c.z = 0f;
        float r2 = radiusWorld * radiusWorld;

        var uniq = new HashSet<GameObject>();

        // ¼¿ ±â¹Ý ÈÄº¸ Ãà¼Ò
        map.GetNavFrame(out _, out var sizeCells, out var cellSize);
        float cellStep = Mathf.Max(Mathf.Abs(cellSize.x), Mathf.Abs(cellSize.y));
        int half = Mathf.Max(0, Mathf.CeilToInt(radiusWorld / Mathf.Max(cellStep, 0.0001f)));
        var centerCell = map.WorldToCell(c);

        if ((targetKind & TargetKind.Towers) != 0)
        {
            var rect = AcquireTargetsRectCells(centerCell, half, TargetKind.Towers);
            for (int i = 0; i < rect.Count; i++)
            {
                var t = rect[i];
                if (t == null || !t.activeInHierarchy) continue;
                if ((t.transform.position - c).sqrMagnitude <= r2 && uniq.Add(t)) result.Add(t);
            }
        }

        if ((targetKind & TargetKind.Monsters) != 0)
        {
            var monsters = GameObject.FindGameObjectsWithTag(_monsterTag);
            for (int i = 0; i < monsters.Length; i++)
            {
                var m = monsters[i];
                if (m == null || !m.activeInHierarchy) continue;
                if ((m.transform.position - c).sqrMagnitude <= r2 && uniq.Add(m)) result.Add(m);
            }
        }

        return result;
    }
    // SkillManager ³»ºÎ¿¡ µÎ¸é ÆíÇÔ
    public bool TryGetSkillDef(string id, out SkillDef def)
    {
        def = default;

        var list = DataManager.Instance?.SkillDatas;
        if (list == null || string.IsNullOrEmpty(id)) return false;

        // struct/class ±¸ºÐ ¾øÀÌ ¾ÈÀüÇÑ Ã£±â
        int idx = list.FindIndex(s => string.Equals(s.Id, id, StringComparison.OrdinalIgnoreCase));
        if (idx < 0) return false;

        var data = list[idx];

        def = new SkillDef
        {
            Id = data.Id,
            Cost = Mathf.RoundToInt(data.Cost), // floatÀÌ¸é int·Î º¯È¯
            Value = data.Value,
            Cooltime = data.Cooltime,
            Duration = data.Duration
        };
        return true;
    }

    public bool CanAfford(in SelectedSkill sel)
    {
        if (!TryGetDef(sel.Id, out var def)) return false;
        if (CostManager.Instance == null) return true;
        return CostManager.Instance.CurrentSkill >= def.Cost;
    }
    public bool TryGetCooldownInfo(int slotIndex, out float remain, out float total, out float ratio)
    {
        remain = 0f; total = 0f; ratio = 0f;

        if (slotIndex < 0 || slotIndex >= _loadout.Count) return false;
        if (!TryGetDef(_loadout[slotIndex].Id, out var def)) return false;

        total = Mathf.Max(0f, def.Cooltime);

        if (_slotCooldownEnd.TryGetValue(slotIndex, out var end))
            remain = Mathf.Max(0f, end - Time.time);  // (unscaledTimeÀ» ¾²¸é ÀÏ½ÃÁ¤Áö Áß¿¡µµ °¨¼Ò)
        else
            remain = 0f;

        ratio = (total > 0f) ? (remain / total) : 0f; // 1¡æ0À¸·Î ³»·Á°¨
        return true;
    }

    public bool IsCoolingDown(int slotIndex)
    {
        return TryGetCooldownInfo(slotIndex, out var remain, out _, out _) && remain > 0.001f;
    }
    public void ResetAllCooldowns()
    {
        // Äð´Ù¿î Á¤º¸ ÀüºÎ Á¦°Å ¡æ ¸ðµÎ Áï½Ã »ç¿ë °¡´É »óÅÂ
        _slotCooldownEnd.Clear();
    }

    // (¼±ÅÃ) Æ¯Á¤ ½½·Ô¸¸ ÁØºñ »óÅÂ·Î ¸¸µé°í ½ÍÀ» ¶§
    public void ResetCooldownOfSlot(int slotIndex)
    {
        _slotCooldownEnd.Remove(slotIndex);
    }
    public void StartAllCooldownsFromDefs(bool useCsvCooltime = true, float fixedSeconds = 0f)
    {
        _slotCooldownEnd.Clear(); // ÀÌÀü ½ºÅ×ÀÌÁö ÈçÀû Á¦°Å

        for (int i = 0; i < _loadout.Count; i++)
        {
            float cd = 0f;

            if (useCsvCooltime)
            {
                if (TryGetDef(_loadout[i].Id, out var def))
                    cd = Mathf.Max(0f, def.Cooltime);
            }
            else
            {
                cd = Mathf.Max(0f, fixedSeconds);
            }

            if (cd > 0f)
                _slotCooldownEnd[i] = Time.time + cd; // ½ÃÀÛÇÏÀÚ¸¶ÀÚ Äð´Ù¿î ON
        }
    }
}
