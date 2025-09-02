using UnityEngine;

public class SmashWall : BossSkillBase
{
    [Header("VFX/SFX (¿É¼Ç)")]
    [SerializeField] private ParticleSystem _fxPrefab; 
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

                    
                    if (_fxPrefab != null)
                    {
                        Vector3 pos = _map.CellToWorld(r, c);
                        pos.z = 0f;
                        var fx = Object.Instantiate(_fxPrefab, pos, Quaternion.identity);
                        Object.Destroy(fx.gameObject, 2f); 
                    }

                    if (_sfx) _sfx.Play();

                    return;
                }
            }
        }
    }
}
