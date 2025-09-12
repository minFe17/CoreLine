using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Utils;

/// 빌드 옵션 1개에 대한 데이터
public struct TowerOption
{
    public string Id;
    public Sprite Icon;
    public GameObject Prefab;
    public int Cost;

    public TowerOption(string id, Sprite icon, GameObject prefab, int cost)
    { Id = id; Icon = icon; Prefab = prefab; Cost = cost; }
}

/// BuildUI: 타일 기준 원형 배치 (원형 우선 → 화면 부족시 한쪽 반원 2줄 폴백)
public class BuildUI : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────────────
    // 레이아웃(간격 2.5배 느낌)
    // ─────────────────────────────────────────────────────────────────────
    private const float BUTTON_SPACING = 60f;   // 버튼들 사이 여유(호 길이 기준)
    private const float ROW_GAP = 120f;  // 반원 2줄일 때 안쪽/바깥쪽 간격
    private const float SCREEN_PADDING = 36f;   // 화면 가장자리 여유

    [SerializeField] bool closeOnBackground = true;

    private Canvas _canvas;
    private RectTransform _root;         // BuildPanel
    private RectTransform _dimmer;       // Dimmer(눌러서 닫기)
    private RectTransform _anchor;       // 타일 중심
    private RectTransform _buttonsRoot;  // 버튼 컨테이너

    private Vector3Int _currentCell;
    private bool _hasCurrentCell;

    [Header("Prefabs")]
    [SerializeField] private TowerPlaceButton buttonPrefab;
    private TowerPlaceButton _runtimeButtonPrefab;

    private Action<TowerOption> _onPick;
    private Action<TowerOption, Vector3Int> _onPickCell;

    private bool _wired;

    // ─────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        Wire();
        if (_root) _root.gameObject.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────────────
    // 외부 API (시그니처 동일)
    // ─────────────────────────────────────────────────────────────────────
    public void OpenAtWorld(Vector3 worldPos, List<TowerOption> options)
        => OpenAtWorld(worldPos, options, null);

    public void OpenAtCell(Vector3Int cell, List<TowerOption> options)
        => OpenAtCell(cell, options, null);

    public void OpenAtCell(Vector3Int cell, List<EUnitType> options)
        => OpenAtCell(cell, options, null);

    public void OpenAtWorld(Vector3 worldPos, List<TowerOption> options, Action<TowerOption> onPick)
    {
        if (PauseControl.IsPaused) return;

        _onPick = onPick;
        _onPickCell = null;
        _hasCurrentCell = false;

        if (!_wired) Wire();
        if (!_wired || !_root || !_anchor || !_buttonsRoot) return;

        var map = MapManager.Instance;
        if (map && map.IsReady)
        {
            var cell = map.WorldToCell(worldPos);
            worldPos = map.CellCenterWorld(cell);
            _currentCell = cell;
            _hasCurrentCell = true;
        }

        var uiCam = GetUiCamera();
        var screen = RectTransformUtility.WorldToScreenPoint(uiCam ?? Camera.main, worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screen, uiCam, out var local);
        _anchor.anchoredPosition = local;

        ClearChildren(_buttonsRoot);
        BuildRadialCircleFirst(options);
        _root.gameObject.SetActive(true);
    }

    public void OpenAtWorld(Vector3 worldPos, List<EUnitType> options, Action<TowerOption> onPick)
    {
        if (PauseControl.IsPaused) return;

        _onPick = onPick;
        _onPickCell = null;
        _hasCurrentCell = false;

        if (!_wired) Wire();
        if (!_wired || !_root || !_anchor || !_buttonsRoot) return;

        var map = MapManager.Instance;
        if (map && map.IsReady)
        {
            var cell = map.WorldToCell(worldPos);
            worldPos = map.CellCenterWorld(cell);
            _currentCell = cell;
            _hasCurrentCell = true;
        }

        var uiCam = GetUiCamera();
        var screen = RectTransformUtility.WorldToScreenPoint(uiCam ?? Camera.main, worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screen, uiCam, out var local);
        _anchor.anchoredPosition = local;

        ClearChildren(_buttonsRoot);
        BuildRadialCircleFirst(options);
        _root.gameObject.SetActive(true);
    }

    public void OpenAtCell(Vector3Int cell, List<TowerOption> options, Action<TowerOption, Vector3Int> onPickCell)
    {
        if (PauseControl.IsPaused) return;

        _onPick = null;
        _onPickCell = null; // 캡처로 전달
        _currentCell = cell;
        _hasCurrentCell = true;

        var map = MapManager.Instance;
        var world = map && map.IsReady ? map.CellCenterWorld(cell) : (Vector3)cell;

        OpenAtWorld(world, options, picked => onPickCell?.Invoke(picked, cell));
    }

    public void OpenAtCell(Vector3Int cell, List<EUnitType> options, Action<TowerOption, Vector3Int> onPickCell)
    {
        if (PauseControl.IsPaused) return;

        _onPick = null;
        _onPickCell = null;
        _currentCell = cell;
        _hasCurrentCell = true;

        var map = MapManager.Instance;
        var world = map && map.IsReady ? map.CellCenterWorld(cell) : (Vector3)cell;

        OpenAtWorld(world, options, picked => onPickCell?.Invoke(picked, cell));
    }

    public void Close()
    {
        _onPick = null;
        _onPickCell = null;
        _hasCurrentCell = false;
        if (_root) _root.gameObject.SetActive(false);
        SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorType.EndSelectTile);
    }

    // ─────────────────────────────────────────────────────────────────────
    // 원형 우선 → 한쪽 반원 2줄 폴백
    // ─────────────────────────────────────────────────────────────────────
    private void BuildRadialCircleFirst(List<TowerOption> options)
    {
        options ??= new List<TowerOption>();

        Vector2 cellSize = GetPrefabButtonSize(_runtimeButtonPrefab, new Vector2(80f, 120f));
        float btnRadius = 0.5f * Mathf.Max(cellSize.x, cellSize.y);
        float baseRadius = btnRadius + BUTTON_SPACING + 10f;

        var positions = ComputeCircleFirstPositions(options.Count, baseRadius, cellSize);

        for (int i = 0; i < positions.Count; i++)
        {
            var b = Instantiate(_runtimeButtonPrefab, _buttonsRoot);
            b.gameObject.SetActive(true);

            var rt = (RectTransform)b.transform;
            rt.sizeDelta = cellSize;
            rt.anchoredPosition = _anchor.anchoredPosition + positions[i];

            if (i < options.Count)
            {
                var opt = options[i];
                b.Bind(opt, picked =>
                {
                    if (_onPickCell != null && _hasCurrentCell)
                        _onPickCell.Invoke(picked, _currentCell);
                    else
                        _onPick?.Invoke(picked);
                    Close();
                });
            }
            else b.Bind(default(TowerOption), null);
        }
    }

    private void BuildRadialCircleFirst(List<EUnitType> options)
    {
        options ??= new List<EUnitType>();

        Vector2 cellSize = GetPrefabButtonSize(_runtimeButtonPrefab, new Vector2(80f, 120f));
        float btnRadius = 0.5f * Mathf.Max(cellSize.x, cellSize.y);
        float baseRadius = btnRadius + BUTTON_SPACING + 10f;

        var positions = ComputeCircleFirstPositions(options.Count, baseRadius, cellSize);

        for (int i = 0; i < positions.Count; i++)
        {
            var b = Instantiate(_runtimeButtonPrefab, _buttonsRoot);
            b.gameObject.SetActive(true);

            var rt = (RectTransform)b.transform;
            rt.sizeDelta = cellSize;
            rt.anchoredPosition = _anchor.anchoredPosition + positions[i];

            if (i < options.Count)
            {
                var opt = options[i];
                b.Bind(opt, picked =>
                {
                    if (_onPickCell != null && _hasCurrentCell)
                        _onPickCell.Invoke(picked, _currentCell);
                    else
                        _onPick?.Invoke(picked);
                    Close();
                });
            }
            else b.Bind(default(EUnitType), null);
        }
    }

    private enum SemiMode { Left, Right, Up, Down }

    // 기본: 정원형(12시 시작, 시계방향). 화면에서 안 맞으면 한쪽 반원 2줄.
    private List<Vector2> ComputeCircleFirstPositions(int count, float baseRadius, Vector2 cellSize)
    {
        var candidates = new List<(List<Vector2> pts, float scale)>(5);

        // 1) 원형도 후보로 넣고, 평행이동/스케일 적용
        var circle = CirclePositions(count, baseRadius, cellSize);
        candidates.Add(FitLayout(circle, cellSize));

        // 2) 4방향 반원 후보들
        foreach (var mode in new[] { SemiMode.Left, SemiMode.Right, SemiMode.Up, SemiMode.Down })
        {
            var semi = TwoRowsOnSemi(count, mode, baseRadius, baseRadius + ROW_GAP);
            candidates.Add(FitLayout(semi, cellSize));
        }

        // 3) 가장 덜 줄여도 되는(=scale 값이 가장 큰) 후보 선택
        (List<Vector2> bestPts, float bestS) = candidates[0];
        for (int i = 1; i < candidates.Count; i++)
            if (candidates[i].scale > bestS) (bestPts, bestS) = candidates[i];

        return bestPts;
    }

    // 12시(-90°) 시작, 시계방향. 인접 호길이 ≥ 버튼폭+여유가 되도록 반지름 보정.
    private List<Vector2> CirclePositions(int n, float baseRadius, Vector2 cellSize)
    {
        var pts = new List<Vector2>(n);
        if (n <= 0) return pts;

        float cell = Mathf.Max(cellSize.x, cellSize.y);
        float delta = 2f * Mathf.PI / n;
        float rNeed = (cell + BUTTON_SPACING * 0.8f) / Mathf.Max(0.001f, delta);
        float r = Mathf.Max(baseRadius, rNeed);

        for (int i = 0; i < n; i++)
        {
            float ang = -90f + (360f / n) * i;       // 12시부터 시계방향
            float rad = ang * Mathf.Deg2Rad;
            pts.Add(new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * r);
        }
        return pts;
    }

    // 한쪽(좌/우) 반원에 2줄(안쪽+바깥쪽). 최대 8개면 4+4 느낌.
    private List<Vector2> TwoRowsOnSemi(int count, SemiMode mode, float rInner, float rOuter)
    {
        var list = new List<Vector2>(count);
        if (count <= 0) return list;

        int inner = Mathf.Min(4, Mathf.CeilToInt(count / 2f));
        int outer = count - inner;

        float start, end;
        switch (mode)
        {
            case SemiMode.Right: start = -90f; end = 90f; break;
            case SemiMode.Left: start = 90f; end = 270f; break;
            case SemiMode.Up: start = 0f; end = 180f; break;
            default: start = 180f; end = 360f; break; // Down
        }

        list.AddRange(PositionsOnArc(inner, start, end, rInner));

        if (outer > 0)
        {
            float shift = (end - start) / (inner + outer + 1f);
            list.AddRange(PositionsOnArc(outer, start + shift, end + shift, rOuter));
        }
        return list;
    }

    // start~end(도) 사이 n개 균등
    private List<Vector2> PositionsOnArc(int n, float startDeg, float endDeg, float r)
    {
        var pts = new List<Vector2>(n);
        if (n <= 0) return pts;

        float d2r = Mathf.Deg2Rad;
        for (int i = 0; i < n; i++)
        {
            float t = (n == 1) ? 0.5f : (i / (n - 1f));
            float ang = Mathf.Lerp(startDeg, endDeg, t);
            pts.Add(new Vector2(Mathf.Cos(ang * d2r), Mathf.Sin(ang * d2r)) * r);
        }
        return pts;
    }

    // 화면 체크
    private bool FitsPositions(List<Vector2> localOffsets, Vector2 cellSize)
    {
        for (int i = 0; i < localOffsets.Count; i++)
        {
            Vector2 local = _anchor.anchoredPosition + localOffsets[i];
            if (!IsInsideScreen(local, cellSize)) return false;
        }
        return true;
    }
    private (List<Vector2> pts, float scale) FitLayout(List<Vector2> pts, Vector2 cellSize)
    {
        // 1) 화면 안으로 먼저 평행이동
        pts = TranslateIntoScreen(new List<Vector2>(pts), cellSize);

        // 2) 그래도 남는 경우에만 동일비율 축소
        float s = EstimateUniformScaleToFit(pts, cellSize); // 1=무축소
        if (s < 1f)
            for (int i = 0; i < pts.Count; i++)
                pts[i] *= s;

        return (pts, s);
    }
    private List<Vector2> TranslateIntoScreen(List<Vector2> points, Vector2 cellSize)
    {
        if (points == null || points.Count == 0) return points;

        float halfW = cellSize.x * 0.5f, halfH = cellSize.y * 0.5f;
        float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
        float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;

        Camera uiCam = GetUiCamera();
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 world = _root.TransformPoint(_anchor.anchoredPosition + points[i]);
            Vector2 sp = RectTransformUtility.WorldToScreenPoint(uiCam, world);
            minX = Mathf.Min(minX, sp.x - halfW);
            maxX = Mathf.Max(maxX, sp.x + halfW);
            minY = Mathf.Min(minY, sp.y - halfH);
            maxY = Mathf.Max(maxY, sp.y + halfH);
        }

        float dx = 0f, dy = 0f;
        if (minX < SCREEN_PADDING) dx += SCREEN_PADDING - minX;
        if (maxX > Screen.width - SCREEN_PADDING) dx -= maxX - (Screen.width - SCREEN_PADDING);
        if (minY < SCREEN_PADDING) dy += SCREEN_PADDING - minY;
        if (maxY > Screen.height - SCREEN_PADDING) dy -= maxY - (Screen.height - SCREEN_PADDING);

        if (dx != 0f || dy != 0f)
        {
            float sf = _canvas ? _canvas.scaleFactor : 1f;
            Vector2 localDelta = new Vector2(dx / Mathf.Max(0.0001f, sf), dy / Mathf.Max(0.0001f, sf));
            for (int i = 0; i < points.Count; i++)
                points[i] += localDelta;
        }
        return points;
    }
    // 모서리 걸리면 반지름만 조금 줄여 안전영역에 맞추기
    private List<Vector2> FinalShrinkToFit(List<Vector2> points, Vector2 cellSize)
    {
        if (points == null || points.Count == 0) return points;

        float s = 1f;                 // 공통 스케일
        const int MAX_ITER = 10;      // 안전 반복

        // s를 점점 줄이면서 전부 화면 안으로 들어갈 때까지 반복
        for (int it = 0; it < MAX_ITER; it++)
        {
            bool ok = true;
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 local = _anchor.anchoredPosition + points[i] * s;
                if (!IsInsideScreen(local, cellSize)) { ok = false; break; }
            }
            if (ok) break;   // 다 들어갔으면 종료
            s *= 0.9f;       // 공통으로 10% 축소
        }

        for (int i = 0; i < points.Count; i++)
            points[i] *= s;

        return points;
    }

    private (float left, float right, float up, float down) GetSideMargins()
    {
        Camera uiCam = GetUiCamera();
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCam, _anchor.position);
        float left = screen.x;
        float right = Screen.width - screen.x;
        float down = screen.y;
        float up = Screen.height - screen.y;
        return (left, right, up, down);
    }

    private bool IsInsideScreen(Vector2 localPoint, Vector2 cellSize)
    {
        Vector3 world = _root.TransformPoint(localPoint);
        Camera uiCam = GetUiCamera();
        Vector2 sp = RectTransformUtility.WorldToScreenPoint(uiCam, world);

        float halfW = cellSize.x * 0.5f;
        float halfH = cellSize.y * 0.5f;

        return (sp.x - halfW) >= SCREEN_PADDING &&
               (sp.x + halfW) <= (Screen.width - SCREEN_PADDING) &&
               (sp.y - halfH) >= SCREEN_PADDING &&
               (sp.y + halfH) <= (Screen.height - SCREEN_PADDING);
    }
    // points를 동일 비율 s(<=1)로 줄였을 때 전부 화면 안에 들어가게 만드는 최소 s를 근사 반환
    private float EstimateUniformScaleToFit(List<Vector2> points, Vector2 cellSize)
    {
        if (points == null || points.Count == 0) return 1f;
        float s = 1f;
        const int MAX_ITER = 12;
        for (int it = 0; it < MAX_ITER; it++)
        {
            bool ok = true;
            for (int i = 0; i < points.Count; i++)
            {
                Vector2 local = _anchor.anchoredPosition + points[i] * s;
                if (!IsInsideScreen(local, cellSize)) { ok = false; break; }
            }
            if (ok) break;
            s *= 0.9f;          // 10%씩 줄이기
        }
        return s;
    }
    // ─────────────────────────────────────────────────────────────────────
    // 배선/유틸
    // ─────────────────────────────────────────────────────────────────────
    private void Wire()
    {
        _wired = false;

        _canvas = GetComponentInParent<Canvas>(true);
        if (!_canvas) _canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);

        _root = (RectTransform)transform;
        if (_canvas && transform.parent != _canvas.transform)
            transform.SetParent(_canvas.transform, worldPositionStays: false);
        StretchToFullScreen(_root);

        _dimmer = FindChildRT(_root, "Dimmer");
        _anchor = FindChildRT(_root, "Anchor");

        _buttonsRoot = FindChildRT(_root, "Buttons");
        if (_buttonsRoot == null)
        {
            var go = new GameObject("Buttons", typeof(RectTransform));
            _buttonsRoot = go.GetComponent<RectTransform>();
            _buttonsRoot.SetParent(_root, false);
            _buttonsRoot.anchorMin = _buttonsRoot.anchorMax = new Vector2(0.5f, 0.5f);
            _buttonsRoot.pivot = new Vector2(0.5f, 0.5f);
            _buttonsRoot.sizeDelta = Vector2.zero;
            _buttonsRoot.anchoredPosition = Vector2.zero;
        }

        if (_dimmer && closeOnBackground)
        {
            var back = _dimmer.GetComponent<Button>();
            if (back)
            {
                back.onClick.RemoveAllListeners();
                back.onClick.AddListener(Close);
            }
        }

        _runtimeButtonPrefab = buttonPrefab
            ? buttonPrefab
            : Resources.Load<TowerPlaceButton>("Map/TowerPlaceButton");

        _wired = (_canvas && _root && _anchor && _buttonsRoot && _runtimeButtonPrefab);
    }

    private Camera GetUiCamera()
    {
        if (_canvas == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
        return _canvas.worldCamera != null ? _canvas.worldCamera : Camera.main;
    }

    static void StretchToFullScreen(RectTransform rect)
    {
        if (!rect) return;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static RectTransform FindChildRT(Transform root, string name)
    {
        if (!root) return null;
        foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
            if (rect.name == name) return rect;
        return null;
    }

    static void ClearChildren(Transform t)
    {
        if (!t) return;
        for (int i = t.childCount - 1; i >= 0; --i)
            UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
    }

    private static Vector2 GetPrefabButtonSize(TowerPlaceButton prefab, Vector2 fallback)
    {
        if (!prefab) return fallback;
        var rt = prefab.GetComponent<RectTransform>();
        if (rt && rt.sizeDelta != Vector2.zero) return rt.sizeDelta;
        return fallback;
    }
}
