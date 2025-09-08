using UnityEngine;

public class PriestAttack : AttackBase
{
    public override void Attack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _unit.UnitStateData.AttackRange);
        Unit targetUnit = null;

        foreach (Collider2D hit in hits)
        {
            Unit unit = hit.GetComponent<Unit>();
            if (unit == null || unit == _unit || unit.CurrentHp >= unit.UnitStateData.HP)
                continue;

            if (targetUnit == null || targetUnit.CurrentHp > unit.CurrentHp)
                targetUnit = unit;
        }

        if (targetUnit != null)
            targetUnit.Heal(_unit.UnitStateData.AttackDamage);
    }

    protected override bool CheckAttack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _unit.UnitStateData.AttackRange);
        foreach (Collider2D hit in hits)
        {
            Unit unit = hit.GetComponent<Unit>();

            if (unit == null || unit == _unit || unit.CurrentHp >= unit.UnitStateData.HP)
                continue;

            return true;
        }
        return false;
    }
}