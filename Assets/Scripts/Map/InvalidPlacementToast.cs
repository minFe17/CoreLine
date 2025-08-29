using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InvalidPlacementToast : MonoBehaviour
{
    [SerializeField] float showSec = 0.7f;
    [SerializeField] float fadeSec = 0.15f;

    Canvas _canvas;
    RectTransform _root;   // 이 스크립트가 붙은 오브젝트(RT)
    CanvasGroup _cg;       // 투명도 제어용
    Coroutine _co;

    void Awake()
    {
        _canvas = GetComponentInParent<Canvas>(true);
        _root = (RectTransform)transform;
        _cg = gameObject.GetComponent<CanvasGroup>();
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();

        // 항상 활성화 상태로 두고 투명하게 숨김
        gameObject.SetActive(true);
        _cg.alpha = 0f;
    }

    public void ShowAtCell(Vector3Int cell)
    {
        var map = MapManager.Instance;
        Vector3 world = map && map.IsReady ? map.CellCenterWorld(cell) : (Vector3)cell;
        ShowAtWorld(world);
    }

    public void ShowAtWorld(Vector3 world)
    {
        // 월드→스크린용 카메라 (항상 실제 카메라)
        Camera worldCam = Camera.main ?? FindFirstObjectByType<Camera>();

        // 스크린→로컬용 UI 카메라 (Overlay면 null, 나머지는 Canvas의 worldCamera 또는 Main)
        Camera uiCam = (_canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? (_canvas.worldCamera != null ? _canvas.worldCamera : worldCam)
            : null;

        // 월드→스크린 (여긴 절대 null 카메라 쓰지 말기!)
        Vector2 screen = worldCam != null
            ? (Vector2)worldCam.WorldToScreenPoint(world)
            : new Vector2(world.x, world.y); // 최후의 방어

        // 부모 기준 스크린→로컬
        RectTransform parent = (RectTransform)_root.parent ?? _root;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screen, uiCam, out var local);

        // 배치
        _root.anchorMin = _root.anchorMax = new Vector2(0.5f, 0.5f);
        _root.pivot = new Vector2(0.5f, 0.5f);
        _root.anchoredPosition = local;

        // 표시
        if (!gameObject.activeSelf) gameObject.SetActive(true);
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(PlayOnce());
    }


    IEnumerator PlayOnce()
    {
        // 페이드 인
        float t = 0f;
        while (t < fadeSec)
        {
            t += Time.deltaTime;
            _cg.alpha = Mathf.Lerp(0f, 1f, t / fadeSec);
            yield return null;
        }
        _cg.alpha = 1f;

        // 잠시 유지
        yield return new WaitForSeconds(showSec);

        // 페이드 아웃 (비활성화하지 말고 알파만 0으로!)
        t = 0f;
        while (t < fadeSec)
        {
            t += Time.deltaTime;
            _cg.alpha = Mathf.Lerp(1f, 0f, t / fadeSec);
            yield return null;
        }
        _cg.alpha = 0f;
        _co = null;
    }
}
