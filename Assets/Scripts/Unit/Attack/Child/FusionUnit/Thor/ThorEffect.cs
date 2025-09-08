using UnityEngine;

public class ThorEffect : MonoBehaviour
{
    [SerializeField] ThorAttack _parent;

    float _damageInterval = 0.5f;

    public int Damage { get => _parent.Unit.UnitStateData.AttackDamage/10; }
    public float DamageInterval { get => _damageInterval; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Monster monster))
            monster.EnterThorEffect(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Monster monster))
            monster.ExitThorEffect();
    }
}