using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Monster))]
[RequireComponent(typeof(Animator))]
public sealed class SpecialExploder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TestMap _map;
    [SerializeField] private string _attackStateName = "Attack";

    [Header("Explosion Rule (Chebyshev Distance)")]
    [SerializeField] private int _rangeCell = 1;
    [SerializeField] private bool _affectDestructibles = true;
    [SerializeField] private bool _affectTowers = true;
    [SerializeField] private float _triggerAtNormalizedTime = 0.05f;

    [Header("VFX / SFX")]
    [SerializeField] private ParticleSystem _explosionFxPrefab;
    [SerializeField] private float _fxLifetime = 1.5f;
    [SerializeField] private string _fxSortingLayer = "Effects";
    [SerializeField] private int _fxOrderInLayer = 200;
    [SerializeField] private AudioSource _sfx;

    private Animator _animator;
    private HealthComponent _stats;
    private bool _armed = false;
    private bool _exploded = false;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _stats = GetComponent<HealthComponent>();
        if (_map == null) { _map = FindAnyObjectByType<TestMap>(); }
    }

    private void OnEnable()
    {
        _armed = true;
        _exploded = false;
    }

    private void Update()
    {
        if (!_armed || _exploded) { return; }
        if (_animator == null) { return; }
        if (_animator.IsInTransition(0)) { return; }

        AnimatorStateInfo st = _animator.GetCurrentAnimatorStateInfo(0);
        if (!st.IsName(_attackStateName)) { return; }

        if (st.normalizedTime >= _triggerAtNormalizedTime)
        {
            Explode();
        }
    }

    public void ExplodeByAnimationEvent()
    {
        if (_exploded) { return; }
        Explode();
    }

    private void Explode()
    {
        if (_exploded) { return; }
        _exploded = true;
        _armed = false;

        if (_map == null)
        {
            if (gameObject.activeSelf) { gameObject.SetActive(false); }
            return;
        }

        Vector2Int center = _map.WorldToCell(transform.position);
        int damage = GetAttackDamage(); 

        for (int dr = -_rangeCell; dr <= _rangeCell; dr++)
        {
            for (int dc = -_rangeCell; dc <= _rangeCell; dc++)
            {
                int r = center.x + dr;
                int c = center.y + dc;
                if (!_map.InBounds(r, c)) { continue; }

                int chebyshev = Mathf.Max(Mathf.Abs(dr), Mathf.Abs(dc));
                if (chebyshev > _rangeCell) { continue; }

                if (_affectDestructibles && _map.IsDestructible(r, c))
                {
                    _map.SetDestructible(r, c, false);
                }

                if (_affectTowers && damage > 0)
                {
                    if (MapManager.Instance != null)
                    {
                        Vector3 world = _map.CellToWorld(r, c);
                        Vector3Int abs = MapManager.Instance.WorldToCell(world);

                        GameObject towerGo;
                        bool found = MapManager.Instance.TryGetTowerAt(abs, out towerGo);
                        if (found && towerGo != null)
                        {
                            Unit unit = towerGo.GetComponent<Unit>();
                            if (unit != null && !unit.IsDie)
                            {
                                unit.TakeDamage(damage);
                            }
                        }
                    }
                }
            }
        }

        if (_explosionFxPrefab != null)
        {
            ParticleSystem fx = Instantiate(_explosionFxPrefab, transform.position, Quaternion.identity);
            SetupFxSorting(fx.gameObject);
            Destroy(fx.gameObject, _fxLifetime);
        }

        if (_sfx != null)
        {
            _sfx.Play();
        }

        gameObject.SetActive(false);
    }

    private int GetAttackDamage()
    {
        if (_stats != null)
        {
            return Mathf.Max(0, _stats.CurrentAttack);
        }
        return 0;
    }

    private void SetupFxSorting(GameObject go)
    {
        SpriteRenderer[] srs = go.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < srs.Length; i++)
        {
            srs[i].sortingLayerName = _fxSortingLayer;
            srs[i].sortingOrder = _fxOrderInLayer;
        }

        ParticleSystemRenderer[] prs = go.GetComponentsInChildren<ParticleSystemRenderer>(true);
        for (int i = 0; i < prs.Length; i++)
        {
            prs[i].sortingLayerName = _fxSortingLayer;
            prs[i].sortingOrder = _fxOrderInLayer;
        }

        Canvas canvas = go.GetComponentInChildren<Canvas>(true);
        if (canvas != null)
        {
            canvas.sortingLayerName = _fxSortingLayer;
            canvas.sortingOrder = _fxOrderInLayer;
        }
    }
}
