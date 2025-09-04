using UnityEngine;

public class MeteorEffect : MonoBehaviour
{
    [SerializeField] Meteor _parent;

    private void OnParticleSystemStopped()
    {
        _parent.Remove();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out Monster monster))
        {
            _parent.HitMonster(monster);
            _parent.SpawnSmallMeteor();
        }
    }
}