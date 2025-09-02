using System.Collections;
using UnityEngine;


public abstract class BossSkillBase : MonoBehaviour
{
    [Header("Skill Common")]
    [SerializeField] private float _weight = 1f;
    [SerializeField] private float _cooldown = 6f;
    [SerializeField] private float _castTime = 0.6f;
    [SerializeField] private float _postDelay = 0.2f;

    protected BossController _controller;
    protected BossMonster _boss;
    protected TestMap _map;
    protected MonsterManager _monsterManager;

    protected float _readyAt = 0f;

    public float Weight => _weight;
    public float Cooldown => _cooldown;
    public float CastTime => _castTime;
    public float PostDelay => _postDelay;

    public virtual void Setup(BossController controller)
    {
        _controller = controller;
        _boss = controller ? controller.Boss : null;
        _map = controller ? controller.Map : null;
        _monsterManager = controller ? controller.MonsterManager : null;
    }

    public virtual bool CanCast(BossController controller)
    {
        return Time.time >= _readyAt;
    }

 
    public IEnumerator Execute(BossController controller)
    {
        if (_castTime > 0f) 
            yield return new WaitForSeconds(_castTime);

        
        Perform(controller);
        if (_postDelay > 0f) yield return new WaitForSeconds(_postDelay);

        ArmCooldown();
    }

    protected void ArmCooldown()
    {
        _readyAt = Time.time + _cooldown;
    }

    protected abstract void Perform(BossController controller);
}
