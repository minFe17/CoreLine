using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using Utils; // SimpleSingleton<AddressableManager>

[RequireComponent(typeof(HealthComponent))]
public sealed class MonsterHpBar : MonoBehaviour
{
    [Header("UI Location (prefab에 Canvas가 없을 때만 사용)")]
    [SerializeField] private string _panelTag = "BarPanel";

    [Header("Addressables")]
    [SerializeField] private string _hpBarAddress = "Assets/Prefabs/Monster/Prefab/MonsterHpBar.prefab";

    [Header("World Offset -> Screen 보정")]
    [SerializeField] private Vector2 _screenOffset = new Vector2(0f, 1.2f);

    private HealthComponent _health;
    private AddressableManager _addrMgr;

    private GameObject _hpBarPrefabAsset;   // Addressables로 로드된 프리팹(Release 대상)
    private GameObject _hpBarInstance;      // 인스턴스
    private HpBar _hpBarScript;             // 프리팹 내부 스크립트

    private bool _isLoadingPrefab = false;

    private void Awake()
    {
        _health = GetComponent<HealthComponent>();
        _addrMgr = SimpleSingleton<AddressableManager>.Instance;
    }

    private void OnEnable()
    {
        TryCreateBar();

        if (_health != null)
        {
            _health.OnHealthChanged += OnHealthChanged;
            _health.OnDied += OnDied;
        }

        UpdatePosition();
        ApplyFillInstant();
    }

    private void OnDisable()
    {
        if (_health != null)
        {
            _health.OnHealthChanged -= OnHealthChanged;
            _health.OnDied -= OnDied;
        }

        DestroyBarInstance();

        if (_hpBarPrefabAsset != null)
        {
            _addrMgr.Release(_hpBarPrefabAsset);
            _hpBarPrefabAsset = null;
        }
        _isLoadingPrefab = false;
    }

    private void LateUpdate()
    {
        TryCreateBar();
        UpdatePosition();
    }

    private void TryCreateBar()
    {
        if (_hpBarInstance != null) { return; }
        if (_isLoadingPrefab) { return; }
        if (string.IsNullOrWhiteSpace(_hpBarAddress)) { return; }

        StartCoroutine(CoLoadAndInstantiate());
    }

    private IEnumerator CoLoadAndInstantiate()
    {
        _isLoadingPrefab = true;

        Task<GameObject> task = _addrMgr.GetAddressableAsset<GameObject>(_hpBarAddress);
        while (!task.IsCompleted) { yield return null; }
        _hpBarPrefabAsset = task.Result;

        _isLoadingPrefab = false;

        if (_hpBarPrefabAsset == null)
        {
            Debug.LogError("[MonsterHpBar] Addressables load failed. Address=" + _hpBarAddress);
            yield break;
        }

        // 프리팹에 Canvas가 있으면 그대로 루트에 생성, 없으면 BarPanel 아래에 생성
        Transform parent = null;
        Canvas prefabCanvas = _hpBarPrefabAsset.GetComponentInChildren<Canvas>(true);
        if (prefabCanvas == null)
        {
            GameObject panel = GameObject.FindGameObjectWithTag(_panelTag);
            if (panel != null) { parent = panel.transform; }
        }

        _hpBarInstance = Object.Instantiate(_hpBarPrefabAsset, parent);
        _hpBarScript = _hpBarInstance.GetComponentInChildren<HpBar>(true);

        if (_hpBarScript == null)
        {
            Debug.LogWarning("[MonsterHpBar] HpBar component not found in prefab: " + _hpBarAddress);
        }

        ApplyFillInstant();
        UpdatePosition();
    }

    private void DestroyBarInstance()
    {
        if (_hpBarInstance != null)
        {
            Object.Destroy(_hpBarInstance);
            _hpBarInstance = null;
            _hpBarScript = null;
        }
    }

    private void OnHealthChanged(int cur, int max)
    {
        if (_hpBarScript == null) { return; }
        float ratio = (max > 0) ? ((float)cur / (float)max) : 0f;
        _hpBarScript.ChangeHp(ratio);
    }

    private void OnDied()
    {
        DestroyBarInstance();
    }

    private void UpdatePosition()
    {
        if (_hpBarInstance == null) { return; }
        if (_hpBarScript == null) { return; }

        Camera cam = Camera.main;
        if (cam == null) { return; }

        Vector3 world = transform.position + new Vector3(_screenOffset.x, _screenOffset.y, 0f);
        Vector3 screen = cam.WorldToScreenPoint(world);

        // HpBar.cs는 스크린 좌표를 그대로 받는 SetPosition(Vector3)를 사용
        _hpBarScript.SetPosition(screen);
    }

    private void ApplyFillInstant()
    {
        if (_hpBarScript == null || _health == null) { return; }
        float ratio = (_health.MaxHp > 0) ? ((float)_health.CurrentHp / (float)_health.MaxHp) : 0f;
        _hpBarScript.ChangeHp(ratio);
    }
}
