using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class ObjectTile : MonoBehaviour, IPointerClickHandler
{
    [Header("Fade Out (Natural)")]
    [SerializeField] private float fadeDuration = 0.45f;   // 총 페이드 시간
    [SerializeField] private float endScale = 0.94f;       // 살짝 축소
    [SerializeField] private bool useUnscaledTime = false; // 타임스케일 무시 여부

    private Vector3Int _cell;
    private bool _registered;
    private bool _isFading;
    private MapManager _map;

    private static bool s_quitting = false;

    private void Start()
    {
        _map = MapManager.Instance;
        if (_map != null && _map.IsReady)
        {
            _cell = _map.WorldToCell(transform.position);
            _map.MarkOccupied(_cell);
            _registered = true;
        }
        else
        {
            Debug.LogWarning("[ObjectTile] MapManager가 아직 준비되지 않았습니다.");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (PauseControl.IsPaused) return;
        Activate();
    }

    public void Activate()
    {
        if (_isFading) return;

        if (PauseControl.IsPaused) return;

        // 코스트 지급
        CostManager.Instance.Add(30);

        // 논리 점유는 즉시 해제
        if (_registered && _map != null)
        {
            _map.UnmarkOccupied(_cell);
            _registered = false;
        }

        // 더 이상 상호작용 금지
        var col = GetComponent<Collider2D>();
        if (col) col.enabled = false;

        StartCoroutine(FadeOutThenDestroy());
    }

    private IEnumerator FadeOutThenDestroy()
    {
        _isFading = true;

        // 타겟들 수집
        var renderers = GetComponentsInChildren<SpriteRenderer>(true);
        var startColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++) if (renderers[i]) startColors[i] = renderers[i].color;

        var groups = GetComponentsInChildren<CanvasGroup>(true);
        var startAlphas = new float[groups.Length];
        for (int i = 0; i < groups.Length; i++) if (groups[i]) startAlphas[i] = groups[i].alpha;

        // 스케일만 자연스럽게 축소
        Vector3 startSc = transform.localScale;
        Vector3 endSc = startSc * Mathf.Clamp(endScale, 0.5f, 1f);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);

            // S-curve로 부드럽게
            float eased = Mathf.SmoothStep(0f, 1f, k);
            float alpha = 1f - eased; // 1→0

            // 알파 적용
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                if (!r) continue;
                Color c = startColors[i];
                c.a = startColors[i].a * alpha;
                r.color = c;
            }
            for (int i = 0; i < groups.Length; i++)
            {
                var g = groups[i];
                if (!g) continue;
                g.alpha = startAlphas[i] * alpha;
            }

            // 스케일만 보간
            transform.localScale = Vector3.Lerp(startSc, endSc, eased);

            yield return null;
        }

        // 최종값 보정
        for (int i = 0; i < renderers.Length; i++)
            if (renderers[i]) { var c = renderers[i].color; c.a = 0f; renderers[i].color = c; }
        for (int i = 0; i < groups.Length; i++)
            if (groups[i]) groups[i].alpha = 0f;

        transform.localScale = endSc;

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (s_quitting) return;
        if (_registered && _map != null)
        {
            _map.UnmarkOccupied(_cell);
            _registered = false;
        }
    }

    private void OnApplicationQuit() => s_quitting = true;
}
