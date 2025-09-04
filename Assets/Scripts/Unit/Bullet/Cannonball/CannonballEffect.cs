using UnityEngine;

public class CannonballEffect : MonoBehaviour
{
    [SerializeField] Cannonball _parent;

    private void OnParticleSystemStopped()
    {
        _parent.Remove();
        this.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Monster monster))
            _parent.HitMonster(monster);
    }
}