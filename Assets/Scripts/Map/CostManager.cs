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
        startValue = 0
    };

    [Header("Skill Budget")]
    [SerializeField]
    private WalletSettings skillSettings = new WalletSettings
    {
        autoGain = true,
        gainPerSecond = 2f,
        useUnscaledTime = false,
        startValue = 0
    };

    // ───────────────────────────────
    // 내부 공통 지갑
    // ───────────────────────────────
    private class Wallet
    {
        public int current;
        public float carry;
        public WalletSettings settings;

        public Wallet(WalletSettings walletSettings)
        {
            settings = walletSettings;
            current = Mathf.Max(0, walletSettings.startValue);
            carry = 0f;
        }
    }

    private Wallet[] wallets;

    public int CurrentUnit { get { return wallets[(int)CostType.Unit].current; } }
    public int CurrentSkill { get { return wallets[(int)CostType.Skill].current; } }

    // 이벤트
    public event Action<int> OnUnitChanged;
    public event Action<int> OnSkillChanged;
    public event Action<CostType, int> OnChanged;

    private void Awake()
    {
        wallets = new Wallet[]
        {
            new Wallet(unitSettings),
            new Wallet(skillSettings)
        };
    }

    private void OnEnable()
    {
        OnUnitChanged?.Invoke(CurrentUnit);
        OnSkillChanged?.Invoke(CurrentSkill);
        OnChanged?.Invoke(CostType.Unit, CurrentUnit);
        OnChanged?.Invoke(CostType.Skill, CurrentSkill);
    }

    private void Update()
    {
        for (int i = 0; i < wallets.Length; i++)
        {
            CostType type = (CostType)i;
            Wallet wallet = wallets[i];

            float deltaTime = wallet.settings.useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (wallet.settings.autoGain && wallet.settings.gainPerSecond > 0f && deltaTime > 0f)
            {
                wallet.carry += wallet.settings.gainPerSecond * deltaTime;
                if (wallet.carry >= 1f)
                {
                    int increase = Mathf.FloorToInt(wallet.carry);
                    wallet.carry -= increase;
                    Add(type, increase);
                }
            }
        }
    }

    // ───────────────────────────────
    // 공통 API
    // ───────────────────────────────
    public int GetCurrent(CostType type)
    {
        return wallets[(int)type].current;
    }

    public void Add(CostType type, int amount)
    {
        if (amount == 0) return;
        Wallet wallet = wallets[(int)type];
        wallet.current = Mathf.Max(0, wallet.current + amount);
        wallets[(int)type] = wallet;
        EmitChanged(type, wallet.current);
    }

    public void Add(CostType type, float amountFraction)
    {
        Wallet wallet = wallets[(int)type];
        wallet.carry += amountFraction;
        if (wallet.carry >= 1f)
        {
            int increase = Mathf.FloorToInt(wallet.carry);
            wallet.carry -= increase;
            wallets[(int)type] = wallet;
            Add(type, increase);
        }
        else
        {
            wallets[(int)type] = wallet;
        }
    }

    public void SetValue(CostType type, int value)
    {
        Wallet wallet = wallets[(int)type];
        wallet.current = Mathf.Max(0, value);
        wallets[(int)type] = wallet;
        EmitChanged(type, wallet.current);
    }

    public bool TrySpend(CostType type, int amount)
    {
        if (amount <= 0) return true;
        Wallet wallet = wallets[(int)type];
        if (wallet.current < amount) return false;
        wallet.current -= amount;
        wallets[(int)type] = wallet;
        EmitChanged(type, wallet.current);
        return true;
    }

    public void SpendForce(CostType type, int amount)
    {
        if (amount <= 0) return;
        Wallet wallet = wallets[(int)type];
        wallet.current = Mathf.Max(0, wallet.current - amount);
        wallets[(int)type] = wallet;
        EmitChanged(type, wallet.current);
    }

    public void SetAutoGain(CostType type, bool enabled)
    {
        Wallet wallet = wallets[(int)type];
        wallet.settings.autoGain = enabled;
        wallets[(int)type] = wallet;
    }

    public void SetGainRate(CostType type, float perSecond)
    {
        Wallet wallet = wallets[(int)type];
        wallet.settings.gainPerSecond = Mathf.Max(0f, perSecond);
        wallets[(int)type] = wallet;
    }

    public void SetUseUnscaled(CostType type, bool useUnscaled)
    {
        Wallet wallet = wallets[(int)type];
        wallet.settings.useUnscaledTime = useUnscaled;
        wallets[(int)type] = wallet;
    }

    // ───────────────────────────────
    // 하위 호환 API
    // ───────────────────────────────
    public void AddUnit(int amount) { Add(CostType.Unit, amount); }
    public void AddUnitFraction(float amount) { Add(CostType.Unit, amount); }
    public void SetUnitValue(int value) { SetValue(CostType.Unit, value); }
    public bool TrySpendUnit(int amount) { return TrySpend(CostType.Unit, amount); }
    public void SpendUnitForce(int amount) { SpendForce(CostType.Unit, amount); }
    public void SetUnitAutoGain(bool enabled) { SetAutoGain(CostType.Unit, enabled); }
    public void SetUnitGainRate(float perSecond) { SetGainRate(CostType.Unit, perSecond); }

    public void AddSkill(int amount) { Add(CostType.Skill, amount); }
    public void AddSkillFraction(float amount) { Add(CostType.Skill, amount); }
    public void SetSkillValue(int value) { SetValue(CostType.Skill, value); }
    public bool TrySpendSkill(int amount) { return TrySpend(CostType.Skill, amount); }
    public void SpendSkillForce(int amount) { SpendForce(CostType.Skill, amount); }
    public void SetSkillAutoGain(bool enabled) { SetAutoGain(CostType.Skill, enabled); }
    public void SetSkillGainRate(float perSecond) { SetGainRate(CostType.Skill, perSecond); }

    // ───────────────────────────────
    // 이벤트 브릿지
    // ───────────────────────────────
    private void EmitChanged(CostType type, int value)
    {
        OnChanged?.Invoke(type, value);
        if (type == CostType.Unit) OnUnitChanged?.Invoke(value);
        else OnSkillChanged?.Invoke(value);
    }
}
