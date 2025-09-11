using UnityEngine;

public class ClericAttack : AttackBase
{
    public override void Attack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _unit.UnitStateData.AttackRange);

        foreach (Collider2D hit in hits)
        {
            Unit unit = hit.GetComponent<Unit>();
            if (unit == null || unit == _unit || unit.CurrentHp >= unit.UnitStateData.HP)
                continue;
            unit.Heal(_unit.UnitStateData.AttackDamage);
        }
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