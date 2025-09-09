using UnityEngine;

public sealed class SmashWall : BossSkillBase
{
    [Header("VFX/SFX (옵션)")]
    [SerializeField] private ParticleSystem _fxPrefab;
    [SerializeField] private float _fxLifetimeOverride = 0f;
    [SerializeField] private string _fxSortingLayer = "Effects";
    [SerializeField] private int _fxSortingOrder = 120;
    [SerializeField] private AudioSource _sfx;

    private Vector2Int _cachedTarget;
    private bool _hasTarget = false;

    private bool InBoundsRC(int r, int c)
    {
        return _map != null && r >= 0 && r < _map.Height && c >= 0 && c < _map.Width;
    }

    public override bool CanCast(BossController controller)
    {
        if (_map == null) { return false; }

        _hasTarget = false;

        for (int r = 0; r < _map.Height; r++)
        {
            for (int c = 0; c < _map.Width; c++)
            {
                if (_map.IsDestructible(r, c))
                {
                    _cachedTarget = new Vector2Int(r, c);
                    _hasTarget = true;
                    return true;
                }
            }
        }
        return false;
    }

    protected override void Perform(BossController controller)
    {
        if (_map == null) { return; }

        if (_hasTarget
            && InBoundsRC(_cachedTarget.x, _cachedTarget.y)
            && _map.IsDestructible(_cachedTarget.x, _cachedTarget.y))
        {
            // 1) 실제 파괴
            _map.SetDestructible(_cachedTarget.x, _cachedTarget.y, false);

            // 2) 성공 마킹(컨트롤러가 전역 대기/다음 틱 로직을 결정)
            MarkCastSuccess();

            // 3) 이펙트/SFX 출력
            Vector3 pos = _map.CellToWorld(_cachedTarget.x, _cachedTarget.y);
            pos.z = 0f;

            // 3-1) 파티클
            if (_fxPrefab != null)
            {
                ParticleSystem fx = Object.Instantiate<ParticleSystem>(_fxPrefab, pos, Quaternion.identity);

                // 정렬 레이어/오더 적용(가려짐 방지)
                ApplySort(fx.gameObject, _fxSortingLayer, _fxSortingOrder);

                // 수명 계산 후 파괴
                float life = _fxLifetimeOverride;
                if (life <= 0f)
                {
                    ParticleSystem.MainModule main = fx.main;
                    float dur = main.duration;
                    float maxLife = main.startLifetime.mode == ParticleSystemCurveMode.TwoConstants
                        ? main.startLifetime.constantMax
                        : main.startLifetime.constant;
                    life = Mathf.Max(0.1f, dur + maxLife);
                }
                Object.Destroy(fx.gameObject, life);
            }

            // 3-2) 사운드
            if (_sfx != null)
            {
                _sfx.Play();
            }
        }

        _hasTarget = false;
    }

    private void ApplySort(GameObject root, string layer, int order)
    {
        if (root == null) { return; }

        SpriteRenderer[] srs = root.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < srs.Length; i++)
        {
            srs[i].sortingLayerName = layer;
            srs[i].sortingOrder = order;
        }

        ParticleSystemRenderer[] prs = root.GetComponentsInChildren<ParticleSystemRenderer>(true);
        for (int i = 0; i < prs.Length; i++)
        {
            prs[i].sortingLayerName = layer;
            prs[i].sortingOrder = order;
        }

        Canvas canvas = root.GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            canvas.sortingLayerName = layer;
            canvas.sortingOrder = order;
        }
    }
}
