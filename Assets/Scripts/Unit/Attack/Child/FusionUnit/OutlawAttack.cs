using UnityEngine;

public class OutlawAttack : AttackBase
{
    int _attackCount;
    EAttackType _attackType;

    private void Start()
    {
        MapManager.Instance.GetNavFrame(out Vector3Int originCell, out Vector3Int sizeCells, out Vector3 cellSize);
        Debug.Log($"{originCell}, {sizeCells}, {cellSize}");
    }

    protected override void PlayAttackAnimation()
    {
        
        _attackCount++;
        if(_attackCount >= 10)
        {
            _attackType = EAttackType.Skill;
            _unit.Animator.SetTrigger("doSkill");
            _attackCount = 0;
            return;
        }
        _attackType = EAttackType.Attack;
        _unit.Animator.SetTrigger("doAttack");
    }

    public override void Attack()
    {
        // 공격 타입 enum으로?
        if(_attackType == EAttackType.Attack)
        {

        }
        else if(_attackType == EAttackType.Skill)
        {
            
        }
    }
}