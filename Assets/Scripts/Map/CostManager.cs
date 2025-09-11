using System;
using UnityEngine;
using Utils;

public class CostManager : MonoSingleton<CostManager>
{
    public enum CostType { Unit = 0, Skill = 1 }

    [Serializable]
    private struct WalletSettings
    {
        [Header("Auto Gain")]
        public bool autoGain;
        public float gainPerSecond;
        public bool useUnscaledTime;

        [Header("Start")]
        public int startValue;
    }

    [Header("Unit Placement Budget")]
    [SerializeField]
    private WalletSettings unitSettings = new WalletSettings
    {
        autoGain = true,
        gainPerSecond = 2f,
        useUnscaledTime = false,
        startValue = 1000
    };

    [Header("Skill Budget")]
    [SerializeField]
    private WalletSettings skillSettings = new WalletSettings
    {
        autoGain = true,
        gainPerSecond = 2f,
        useUnscaledTime = false,
        startValue = 1000
    };

    [Header("Gate By Player Base")]
    [Tooltip("플레이어 베이스가 설치된 뒤부터 자동 증가하도록 게이트합니다.")]
    [SerializeField] private bool _requireBaseForAutoGain = true;

    private class Wallet
    {
        public int current;
        public float carry;
        public WalletSettings settings;
        public Wallet(WalletSettings s) { settings = s; current = Mathf.Max(0, s.startValue); carry = 0f; }
    }

    private Wallet[] _wallets;

    public int CurrentUnit => _wallets[(int)CostType.Unit].current;
    public int CurrentSkill => _wallets[(int)CostType.Skill].current;

    public event Action<int> OnUnitChanged;
    public event Action<int> OnSkillChanged;
    public event Action<CostType, int> OnChanged;

    // ── 여기 추가: 베이스 설치 게이트 ──
    private bool _basePlacedGate = false;
    private bool _hookedMapEvent = false;

    private void Awake()
    {
        _wallets = new Wallet[] { new Wallet(unitSettings), new Wallet(skillSettings) };
    }

    private void OnEnable()
    {
        // 현재 값 브로드캐스트
        OnUnitChanged?.Invoke(CurrentUnit);
        OnSkillChanged?.Invoke(CurrentSkill);
        OnChanged?.Invoke(CostType.Unit, CurrentUnit);
        OnChanged?.Invoke(CostType.Skill, CurrentSkill);

        TryHookMapEvent();
    }

    private void OnDisable()
    {
        UnhookMapEvent();
    }

    private void TryHookMapEvent()
    {
        if (_hookedMapEvent) return;

        var map = MapManager.Instance;
        if (map != null)
        {
            map.OnPlayerBasePlaced += HandleBasePlaced;
            _hookedMapEvent = true;

            // 이미 설치되어 있으면 즉시 게이트 오픈
            _basePlacedGate = map.HasPlayerBase;
        }
        else
        {
            // MapManager 초기화 대기
            StartCoroutine(CoWaitMapAndHook());
        }
    }

    private System.Collections.IEnumerator CoWaitMapAndHook()
    {
        yield return null;
        while (MapManager.Instance == null) yield return null;

        var map = MapManager.Instance;
        map.OnPlayerBasePlaced += HandleBasePlaced;
        _hookedMapEvent = true;
        _basePlacedGate = map.HasPlayerBase;
    }

    private void UnhookMapEvent()
    {
        if (!_hookedMapEvent) return;
        var map = MapManager.Instance;
        if (map != null) map.OnPlayerBasePlaced -= HandleBasePlaced;
        _hookedMapEvent = false;
    }

    private void HandleBasePlaced(Vector3Int _)
    {
        _basePlacedGate = true;
    }

    private void Update()
    {
        for (int i = 0; i < _wallets.Length; i++)
        {
            CostType type = (CostType)i;
            Wallet wallet = _wallets[i];

            // ── 여기 변경: 베이스 설치 전에는 자동 증가 차단 ──
            bool canAutoGain = wallet.settings.autoGain
                               && (!_requireBaseForAutoGain || _basePlacedGate)
                               && wallet.settings.gainPerSecond > 0f;

            float dt = wallet.settings.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (canAutoGain && dt > 0f)
            {
                wallet.carry += wallet.settings.gainPerSecond * dt;
                if (wallet.carry >= 1f)
                {
                    int inc = Mathf.FloorToInt(wallet.carry);
                    wallet.carry -= inc;
                    Add(type, inc);
                }
            }
        }
    }

    // ── 이하 기존 공통 API 동일 ──
    public int GetCurrent(CostType type) => _wallets[(int)type].current;

    public void Add(CostType type, int amount)
    {
        if (amount == 0) return;
        var w = _wallets[(int)type];
        w.current = Mathf.Max(0, w.current + amount);
        _wallets[(int)type] = w;
        EmitChanged(type, w.current);
    }

    public void Add(CostType type, float amountFraction)
    {
        var w = _wallets[(int)type];
        w.carry += amountFraction;
        if (w.carry >= 1f)
        {
            int inc = Mathf.FloorToInt(w.carry);
            w.carry -= inc;
            _wallets[(int)type] = w;
            Add(type, inc);
        }
        else _wallets[(int)type] = w;
    }

    public void SetValue(CostType type, int value)
    {
        var w = _wallets[(int)type];
        w.current = Mathf.Max(0, value);
        _wallets[(int)type] = w;
        EmitChanged(type, w.current);
    }

    public bool TrySpend(CostType type, int amount)
    {
        if (amount <= 0) return true;
        var w = _wallets[(int)type];
        if (w.current < amount) return false;
        w.current -= amount;
        _wallets[(int)type] = w;
        EmitChanged(type, w.current);
        return true;
    }

    public void SpendForce(CostType type, int amount)
    {
        if (amount <= 0) return;
        var w = _wallets[(int)type];
        w.current = Mathf.Max(0, w.current - amount);
        _wallets[(int)type] = w;
        EmitChanged(type, w.current);
    }

    public void SetAutoGain(CostType type, bool enabled)
    {
        var w = _wallets[(int)type];
        w.settings.autoGain = enabled;
        _wallets[(int)type] = w;
    }
    public void SetGainRate(CostType type, float perSecond)
    {
        var w = _wallets[(int)type];
        w.settings.gainPerSecond = Mathf.Max(0f, perSecond);
        _wallets[(int)type] = w;
    }
    public void SetUseUnscaled(CostType type, bool useUnscaled)
    {
        var w = _wallets[(int)type];
        w.settings.useUnscaledTime = useUnscaled;
        _wallets[(int)type] = w;
    }

    // 하위 호환 래퍼
    public void AddUnit(int a) => Add(CostType.Unit, a); public void AddUnitFraction(float a) => Add(CostType.Unit, a);
    public void SetUnitValue(int v) => SetValue(CostType.Unit, v); public bool TrySpendUnit(int a) => TrySpend(CostType.Unit, a);
    public void SpendUnitForce(int a) => SpendForce(CostType.Unit, a); public void SetUnitAutoGain(bool e) => SetAutoGain(CostType.Unit, e);
    public void SetUnitGainRate(float r) => SetGainRate(CostType.Unit, r);

    public void AddSkill(int a) => Add(CostType.Skill, a); public void AddSkillFraction(float a) => Add(CostType.Skill, a);
    public void SetSkillValue(int v) => SetValue(CostType.Skill, v); public bool TrySpendSkill(int a) => TrySpend(CostType.Skill, a);
    public void SpendSkillForce(int a) => SpendForce(CostType.Skill, a); public void SetSkillAutoGain(bool e) => SetAutoGain(CostType.Skill, e);
    public void SetSkillGainRate(float r) => SetGainRate(CostType.Skill, r);

    private void EmitChanged(CostType type, int value)
    {
        OnChanged?.Invoke(type, value);
        if (type == CostType.Unit) OnUnitChanged?.Invoke(value);
        else OnSkillChanged?.Invoke(value);
    }
}
