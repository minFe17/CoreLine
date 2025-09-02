using UnityEngine;

public class SmashWall : BossSkillBase
{
    [Header("VFX/SFX (¿É¼Ç)")]
    [SerializeField] private ParticleSystem _fx;
    [SerializeField] private AudioSource _sfx;

    protected override void Perform(BossController controller)
    {
        if (_map == null) return;

        for (int r = 0; r < _map.Height; r++)
        {
            for (int c = 0; c < _map.Width; c++)
            {
                if (_map.IsDestructible(r, c))
                {
                    _map.SetDestructible(r, c, false);
                    break;
                }
            }
        }

        if (_fx) _fx.Play();
        if (_sfx) _sfx.Play();
    }
}
