using UnityEngine;
using Utils;

public class EffectBullet : MonoBehaviour
{
    [SerializeField] EBulletType _bulletType;

    private void OnParticleSystemStopped()
    {
        MonoSingleton<ObjectPoolManager>.Instance.Push(_bulletType, gameObject);
    }
}