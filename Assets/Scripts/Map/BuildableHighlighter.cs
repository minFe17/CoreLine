using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // UI 위 클릭 무시용

/// <summary>
/// - 기본: 외곽선 비표시
/// - 아무 타일이든 터치/클릭 시: 빌드 가능 전체 외곽선 표시
/// - 그때 탭한 타일이 빌드 가능이면 해당 타일만 반짝
/// - (선택) autoHideSec > 0 이면 일정 시간 뒤 자동으로 감춤
/// </summary>
[DefaultExecutionOrder(1000)]
public class BuildableHighlighter : MonoBehaviour
{
    [Header("일반 외곽선(전체 표시시)")]
    [SerializeField] Color baseColor = new Color(0f, 1f, 0.6f, 0.55f);
    [SerializeField] float baseWidth = 0.03f;

    [Header("강조(선택 셀)")]
    [SerializeField] Color hoverColor = new Color(0.2f, 1f, 0.9f, 1f);
    [SerializeField] float hoverWidth = 0.05f;
    [SerializeField] bool hoverPulse = true;
    [SerializeField] float pulseSpeed = 2.2f;      // 깜빡임 속도
    [SerializeField] float pulseWidthAmp = 0.015f; // 두께 펌핑
    [SerializeField] float pulseAlphaAmp = 0.25f;  // 알파 펌핑(0~1 가중치)

    [Header("표시/숨김 옵션")]
    [SerializeField] float autoHideSec = 1f; // -1이면 자동 숨김 안함

    [Header("정렬(2D 기준)")]
    [SerializeField] string sortingLayerName = "Default";
    [SerializeField] int sortingOrder = 1000;

    private Material _baseMat;
    private Material _hoverMat;

    private readonly List<LineRenderer> _pool = new();
    private int _used; // 이번 프레임에 사용된 개수
    private LineRenderer _hoverLR;

    private MapManager _map;
    private Camera _cam;
    private Vector3 _cellSize = Vector3.one;

    // 상태
    private bool _showAll = false;
    private Vector3Int _selectedCell;
    private bool _selectedPlaceable = false;
    private float _hideAt = -1f;

    private void Awake()
    {
        _map = MapManager.Instance;
        _cam = Camera.main;

        var shader = Shader.Find("Sprites/Default");
        _baseMat = new Material(shader); _baseMat.color = baseColor;
        _hoverMat = new Material(shader); _hoverMat.color = hoverColor;
    }

    private void OnEnable()
    {
        if (_map != null) _map.OnCellChanged += OnCellChanged;
    }
    private void OnDisable()
    {
        if (_map != null) _map.OnCellChanged -= OnCellChanged;
    }

    private void Start()
    {
        if (_map == null || !_map.IsReady) return;
        _map.GetNavFrame(out _, out _, out _cellSize);
        ClearAllImmediate();
    }

    private void Update()
    {
        if (_map == null || !_map.IsReady) return;

        // 입력 체크(마우스 & 터치)
        if (JustTapped(out Vector3 world) && !IsPointerOverUI())
        {
            world.z = 0f;
            _selectedCell = _map.WorldToCell(world);
            _selectedPlaceable = _map.GetPlaceInfo(_selectedCell).Placeable;

            // 표시 켜기
            _showAll = true;
            if (autoHideSec > 0f) _hideAt = Time.unscaledTime + autoHideSec;
        }

        // 자동 숨김
        if (_showAll && autoHideSec > 0f && Time.unscaledTime >= _hideAt)
        {
            ClearAllImmediate();
        }

        // 풀 재사용 카운트 초기화
        _used = 0;

        if (_showAll)
        {
            DrawAllBuildables();
            UpdateHoverSelected();
        }
        else
        {
            // 사용 안 한 외곽선 전부 끄기
            DisableUnusedPool();
            if (_hoverLR) _hoverLR.enabled = false;
        }
    }

    // 맵 셀 상태 변경 시: 켜져있는 경우 다시 그리기
    private void OnCellChanged(Vector3Int _)
    {
        if (_showAll)
        {
            // 다음 Update에서 다시 배치되므로 여기선 풀만 정리
            // (필요 시 확장 가능)
        }
    }

    // ─────────────────────────────────────────────────────────
    // 전체 빌드가능 외곽선
    // ─────────────────────────────────────────────────────────
    private void DrawAllBuildables()
    {
        BoundsInt bounds = _map.GetNavBounds();
        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            MapManager.PlaceInfo info = _map.GetPlaceInfo(cell);
            if (!info.Placeable) continue;

            Vector3 center = _map.CellCenterWorld(cell);
            LineRenderer linerenderer = GetLR();
            SetupBaseLR(linerenderer);
            SetSquare(linerenderer, center, _cellSize);
            linerenderer.enabled = true;
        }

        DisableUnusedPool();
    }

    // ─────────────────────────────────────────────────────────
    // 선택 셀 강조(펄스)
    // ─────────────────────────────────────────────────────────
    private void UpdateHoverSelected()
    {
        if (_hoverLR == null)
        {
            _hoverLR = CreateLR("HoverOutline");
            _hoverLR.material = _hoverMat;
            _hoverLR.sortingLayerName = sortingLayerName;
            _hoverLR.sortingOrder = sortingOrder + 1;
            _hoverLR.enabled = false;
        }

        if (!_selectedPlaceable)
        {
            _hoverLR.enabled = false;
            return;
        }

        // 펄스(두께 + 알파)
        float width = hoverWidth;
        float alphaMul = 1f;
        if (hoverPulse)
        {
            float s = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * pulseSpeed);
            width += (s - 0.5f) * 2f * pulseWidthAmp;
            alphaMul = Mathf.Clamp01(1f - pulseAlphaAmp + s * pulseAlphaAmp);
        }

        Color c = hoverColor; c.a *= alphaMul;
        _hoverLR.material.color = c;
        _hoverLR.widthMultiplier = width;

        Vector3 center = _map.CellCenterWorld(_selectedCell);
        SetSquare(_hoverLR, center, _cellSize);
        _hoverLR.enabled = true;
    }

    // ─────────────────────────────────────────────────────────
    // LineRenderer 풀/생성/셋업
    // ─────────────────────────────────────────────────────────
    private LineRenderer GetLR()
    {
        if (_used < _pool.Count)
            return _pool[_used++];

        LineRenderer lineRenderer = CreateLR($"BuildableOutline_{_pool.Count}");
        _pool.Add(lineRenderer);
        _used++;
        return lineRenderer;
    }

    private LineRenderer CreateLR(string name)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.SetParent(transform, false);
        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
        lineRenderer.textureMode = LineTextureMode.Stretch;
        lineRenderer.numCornerVertices = 2;
        lineRenderer.numCapVertices = 0;
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.sortingLayerName = sortingLayerName;
        lineRenderer.sortingOrder = sortingOrder;
        lineRenderer.enabled = false;
        return lineRenderer;
    }

    private void SetupBaseLR(LineRenderer lineRenderer)
    {
        lineRenderer.material = _baseMat;
        lineRenderer.widthMultiplier = baseWidth;
    }

    static readonly Vector3[] _pts = new Vector3[5];
    private void SetSquare(LineRenderer lineRenderer, Vector3 center, Vector3 size)
    {
        float hx = size.x * 0.5f;
        float hy = size.y * 0.5f;

        _pts[0] = new Vector3(center.x - hx, center.y - hy, 0f);
        _pts[1] = new Vector3(center.x - hx, center.y + hy, 0f);
        _pts[2] = new Vector3(center.x + hx, center.y + hy, 0f);
        _pts[3] = new Vector3(center.x + hx, center.y - hy, 0f);
        _pts[4] = _pts[0];

        lineRenderer.positionCount = 5;
        lineRenderer.SetPositions(_pts);
    }

    private void DisableUnusedPool()
    {
        for (int i = _used; i < _pool.Count; i++)
            if (_pool[i].enabled) _pool[i].enabled = false;
    }

    private void ClearAllImmediate()
    {
        _showAll = false;
        _selectedPlaceable = false;
        _hideAt = -1f;
        for (int i = 0; i < _pool.Count; i++)
            if (_pool[i]) _pool[i].enabled = false;
        if (_hoverLR) _hoverLR.enabled = false;
    }

    // ─────────────────────────────────────────────────────────
    // 입력 유틸 (마우스/터치 공통 '탭' 체크)
    // ─────────────────────────────────────────────────────────
    private bool JustTapped(out Vector3 world)
    {
        world = default;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            world = _cam ? _cam.ScreenToWorldPoint(Input.mousePosition) : (Vector3)Input.mousePosition;
            return true;
        }
#endif
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                world = _cam ? _cam.ScreenToWorldPoint(touch.position) : (Vector3)touch.position;
                return true;
            }
        }
        return false;
    }

    private bool IsPointerOverUI()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return true;
#endif
        if (Input.touchCount > 0 && EventSystem.current != null)
        {
            Touch touch = Input.GetTouch(0);
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return true;
        }
        return false;
    }
}
