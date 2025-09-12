using UnityEngine;
using Utils;

public class KingUnit : Unit
{
    EUnitType _unitType = EUnitType.King;

    bool _defeatSent = false;

    public int GetHPRatio() => _currentHp / _unitStateData.HP;

    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        if (_unitStateData == null)
            _unitStateData = SimpleSingleton<UnitDataList>.Instance.GetUnitData(_unitType).LevelData[0].UnitState;
        _currentHp = _unitStateData.HP;
        _isDie = false;
        _defeatSent = false;
    }

    public override void ClickUnit()
    {
        SimpleSingleton<AttackRangeManager>.Instance.HideAttackRange();

        SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorType.PlayAudio, ESFXType.Heal);
    }

    public override void TakeDamage(int damage)
    {
        SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorType.PlayAudio, ESFXType.KingHit);   
        base.TakeDamage(damage);
    }

    public override void Die()
    {
        base.Die();
        SendDefeat();
    }

    public override void Remove()
    {
        base.Remove();
        MonoSingleton<ObjectPoolManager>.Instance.Push(_unitType, gameObject);
    }

    private void SendDefeat()
    {
        if (_defeatSent) 
            return;
        _defeatSent = true;

        NormalStageManager nsm = NormalStageManager.Instance;
        if (nsm == null) return;

        NormalStageData stage = nsm.SelectedStage;
        NormalStageManager.StageEndSnapshot snap = ConditionControl.BuildFor(stage);

        nsm.CompleteStageDefeat(snap); //GameManager가 받아서 패배패널 띄움
    }
}