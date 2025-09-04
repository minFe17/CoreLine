using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HealthComponent))]
public sealed class MonsterHpBar : MonoBehaviour
{
    [Header("UI Location")]
    [SerializeField] private string _panelTag = "BarPanel";
    [SerializeField] private string _hpBarPrefabPath = "Monster/Prefab/MonsterHpBar";
    [SerializeField] private Vector2 _screenOffset = new Vector2(0f, 1.2f);

    private HealthComponent _health;
    private GameObject _hpBar;
    private Image _hpFill;

    private void Awake()
    {
        _health = GetComponent<HealthComponent>();
    }

    private void OnEnable()
    {
        TryCreateBar();
        if (_health != null)
        {
            _health.OnHealthChanged += OnHealthChanged;
            _health.OnDied += OnDied;
        }
        UpdatePositionAndFill();
    }

    private void OnDisable()
    {
        if (_health != null)
        {
            _health.OnHealthChanged -= OnHealthChanged;
            _health.OnDied -= OnDied;
        }
        DestroyBar();
    }

    private void LateUpdate()
    {
        UpdatePositionAndFill();
    }

    private void TryCreateBar()
    {
        if (_hpBar != null) { return; }

        GameObject panel = GameObject.FindGameObjectWithTag(_panelTag);
        GameObject prefab = Resources.Load<GameObject>(_hpBarPrefabPath);

        if (panel == null || prefab == null) { return; }

        _hpBar = GameObject.Instantiate(prefab, panel.transform);
        Transform child = _hpBar.transform.GetChild(0);
        _hpFill = child != null ? child.GetComponent<Image>() : null;
    }

    private void DestroyBar()
    {
        if (_hpBar != null)
        {
            GameObject.Destroy(_hpBar);
            _hpBar = null;
            _hpFill = null;
        }
    }

    private void OnHealthChanged(int cur, int max)
    {
        if (_hpFill != null)
        {
            float f = (max > 0) ? ((float)cur / (float)max) : 0f;
            _hpFill.fillAmount = Mathf.Clamp01(f);
        }
    }

    private void OnDied()
    {
        DestroyBar();
    }

    private void UpdatePositionAndFill()
    {
        if (_hpBar == null) { return; }

        Camera cam = Camera.main;
        if (cam == null) { return; }

        Vector3 world = transform.position;
        Vector3 screen = cam.WorldToScreenPoint(world + new Vector3(_screenOffset.x, _screenOffset.y, 0f));
        _hpBar.transform.position = screen;
    }
}
