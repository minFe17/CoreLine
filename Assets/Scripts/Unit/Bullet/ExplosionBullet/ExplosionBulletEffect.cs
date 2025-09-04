using UnityEngine;

public class ExplosionBulletEffect : MonoBehaviour
{
    [SerializeField] ExplosionBullet _parent;

    private void OnParticleSystemStopped()
    {
        _parent.Remove();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Monster monster))
            _parent.HitMonster(monster);
    }
}