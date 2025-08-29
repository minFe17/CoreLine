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

    Material _baseMat;
    Material _hoverMat;

    readonly List<LineRenderer> _pool = new();
    int _used; // 이번 프레임에 사용된 개수
    LineRenderer _hoverLR;

    MapManager _map;
    Camera _cam;
    Vector3 _cellSize = Vector3.one;

    // 상태
    bool _showAll = false;
    Vector3Int _selectedCell;
    bool _selectedPlaceable = false;
    float _hideAt = -1f;

    void Awake()
    {
        _map = MapManager.Instance;
        _cam = Camera.main;

        var shader = Shader.Find("Sprites/Default");
        _baseMat = new Material(shader); _baseMat.color = baseColor;
        _hoverMat = new Material(shader); _hoverMat.color = hoverColor;
    }

    void OnEnable()
    {
        if (_map != null) _map.OnCellChanged += OnCellChanged;
    }
    void OnDisable()
    {
        if (_map != null) _map.OnCellChanged -= OnCellChanged;
    }

    void Start()
    {
        if (_map == null || !_map.IsReady) return;
        _map.GetNavFrame(out _, out _, out _cellSize);
        ClearAllImmediate();
    }

    void Update()
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
    void OnCellChanged(Vector3Int _)
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
    void DrawAllBuildables()
    {
        var bounds = _map.GetNavBounds();
        foreach (var c in bounds.allPositionsWithin)
        {
            var info = _map.GetPlaceInfo(c);
            if (!info.Placeable) continue;

            var center = _map.CellCenterWorld(c);
            var lr = GetLR();
            SetupBaseLR(lr);
            SetSquare(lr, center, _cellSize);
            lr.enabled = true;
        }

        DisableUnusedPool();
    }

    // ─────────────────────────────────────────────────────────
    // 선택 셀 강조(펄스)
    // ─────────────────────────────────────────────────────────
    void UpdateHoverSelected()
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

        var c = hoverColor; c.a *= alphaMul;
        _hoverLR.material.color = c;
        _hoverLR.widthMultiplier = width;

        var center = _map.CellCenterWorld(_selectedCell);
        SetSquare(_hoverLR, center, _cellSize);
        _hoverLR.enabled = true;
    }

    // ─────────────────────────────────────────────────────────
    // LineRenderer 풀/생성/셋업
    // ─────────────────────────────────────────────────────────
    LineRenderer GetLR()
    {
        if (_used < _pool.Count)
            return _pool[_used++];

        LineRenderer lineRenderer = CreateLR($"BuildableOutline_{_pool.Count}");
        _pool.Add(lineRenderer);
        _used++;
        return lineRenderer;
    }

    LineRenderer CreateLR(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = false;
        lr.textureMode = LineTextureMode.Stretch;
        lr.numCornerVertices = 2;
        lr.numCapVertices = 0;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.sortingLayerName = sortingLayerName;
        lr.sortingOrder = sortingOrder;
        lr.enabled = false;
        return lr;
    }

    void SetupBaseLR(LineRenderer lr)
    {
        lr.material = _baseMat;
        lr.widthMultiplier = baseWidth;
    }

    static readonly Vector3[] _pts = new Vector3[5];
    void SetSquare(LineRenderer lr, Vector3 center, Vector3 size)
    {
        float hx = size.x * 0.5f;
        float hy = size.y * 0.5f;

        _pts[0] = new Vector3(center.x - hx, center.y - hy, 0f);
        _pts[1] = new Vector3(center.x - hx, center.y + hy, 0f);
        _pts[2] = new Vector3(center.x + hx, center.y + hy, 0f);
        _pts[3] = new Vector3(center.x + hx, center.y - hy, 0f);
        _pts[4] = _pts[0];

        lr.positionCount = 5;
        lr.SetPositions(_pts);
    }

    void DisableUnusedPool()
    {
        for (int i = _used; i < _pool.Count; i++)
            if (_pool[i].enabled) _pool[i].enabled = false;
    }

    void ClearAllImmediate()
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
    bool JustTapped(out Vector3 world)
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
            var t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                world = _cam ? _cam.ScreenToWorldPoint(t.position) : (Vector3)t.position;
                return true;
            }
        }
        return false;
    }

    bool IsPointerOverUI()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return true;
#endif
        if (Input.touchCount > 0 && EventSystem.current != null)
        {
            var t = Input.GetTouch(0);
            if (EventSystem.current.IsPointerOverGameObject(t.fingerId))
                return true;
        }
        return false;
    }
}
