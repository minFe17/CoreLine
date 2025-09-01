using System;
using UnityEngine;

public class CostManager : MonoBehaviour
{
    public static CostManager Instance => Utils.MonoSingleton<CostManager>.Instance;

    [Header("Income")]
    [SerializeField] private bool autoGain = true;
    [SerializeField] private float gainPerSecond = 2f;   // 1초당 +2
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Starting Value")]
    [SerializeField] private int startValue = 0;

    public int Current { get; private set; }
    public event Action<int> OnChanged;

    float _carry;  // 소수 누적

    void OnEnable()
    {
        // 첫 생성 시 초기화
        Current = startValue;
        OnChanged?.Invoke(Current);
    }

    void Update()
    {
        if (!autoGain || gainPerSecond <= 0f) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _carry += gainPerSecond * dt;           // 초당 2씩 누적
        if (_carry >= 1f)
        {
            int inc = Mathf.FloorToInt(_carry); // 정수 부분만 반영
            _carry -= inc;
            Add(inc);
        }
    }

    // ───── 외부 API ─────
    public void Add(int amount)
    {
        if (amount == 0) return;
        Current = Mathf.Max(0, Current + amount);
        OnChanged?.Invoke(Current);
    }

    public void Add(float amount)   // 소수 추가도 지원 (이자/버프 등)
    {
        _carry += amount;
        if (_carry >= 1f)
        {
            int inc = Mathf.FloorToInt(_carry);
            _carry -= inc;
            Add(inc);
        }
    }
    public void SetValue(int value)
    {
        Current = Mathf.Max(0, value);
        OnChanged?.Invoke(Current);
    }

    /// 충분하면 차감하고 true, 부족하면 false
    public bool TrySpend(int amount)
    {
        if (amount <= 0) return true;
        if (Current < amount) return false;
        Current -= amount;
        OnChanged?.Invoke(Current);
        return true;
    }

    /// 강제 차감(0 미만 방지)
    public void SpendForce(int amount)
    {
        if (amount <= 0) return;
        Current = Mathf.Max(0, Current - amount);
        OnChanged?.Invoke(Current);
    }


    public void SetAutoGain(bool enabled) 
        => autoGain = enabled;
    public void SetGainRate(float perSecond) 
        => gainPerSecond = Mathf.Max(0f, perSecond);
}
