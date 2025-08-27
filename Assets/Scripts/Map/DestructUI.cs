using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DestructUI : MonoBehaviour
{
    [SerializeField] private TowerPlaceButton buttonPrefab; // 버튼 프리팹
    [SerializeField] private float cell = 96f;
    [SerializeField] private float gapFromTile = 16f;
    [SerializeField] private bool closeOnBackground = true;

    private Canvas _canvas;
    private RectTransform _root;
    private RectTransform _dimmer;
    private RectTransform _anchor;
    private RectTransform _leftGrid;
    private RectTransform _rightGrid;

    private Action<TowerOption, Vector3Int> _onPickCell;
    private Vector3Int _currentCell;
    private bool _hasCurrentCell;

    private TowerPlaceButton _runtimeButtonPrefab;
    private bool _wired;
    void Awake()
    {
        Wire();
        if (_root) _root.gameObject.SetActive(false);
    }

    public void OpenAtCell(Vector3Int cell, Sprite cancelIcon, Sprite destroyIcon,
                           Action<TowerOption, Vector3Int> onPickCell)
    {
        // ★ 필요 시 재배선
        if (!_wired) Wire();
        if (!_wired || _root == null || _anchor == null)
        {
            Debug.LogError("[DestructUI] Not wired. Check panel hierarchy and script attachment.");
            return;
        }

        _onPickCell = onPickCell;
        _currentCell = cell;
        _hasCurrentCell = true;

        // 셀 중심 → 스크린 → 로컬 포인트
        var map = MapManager.Instance;
        Vector3 world = map && map.IsReady ? map.CellCenterWorld(cell) : (Vector3)cell;

        Camera uiCam = GetUiCamera();                // Overlay면 null이어야 정상
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCam ?? Camera.main, world);
        Vector2 local;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screen, uiCam, out local))
        {
            Debug.LogWarning("[DestructUI] Failed to convert screen to local.");
            local = Vector2.zero;
        }
        _anchor.anchoredPosition = local;

        // 버튼 정리
        ClearChildren(_leftGrid);
        ClearChildren(_rightGrid);

        // 버튼 배치
        CreateButton(_leftGrid, new TowerOption("cancel", cancelIcon, null, 0));
        CreateButton(_rightGrid, new TowerOption("destroy", destroyIcon, null, 0));

        // 그리드 위치(좌/우 1개씩이므로 간단히 배치)
        PlaceOneByOne(_leftGrid, rightSide: false);
        PlaceOneByOne(_rightGrid, rightSide: true);

        _root.gameObject.SetActive(true);
    }

    public void Close()
    {
        _onPickCell = null;
        _hasCurrentCell = false;
        if (_root) _root.gameObject.SetActive(false);
    }

    // ── 내부 유틸 ──────────────────────────────────────────────
    private void CreateButton(RectTransform parent, TowerOption opt)
    {
        var btn = Instantiate(_runtimeButtonPrefab, parent);
        ((RectTransform)btn.transform).sizeDelta = new Vector2(cell, cell);
        btn.Bind(opt, picked =>
        {
            if (_hasCurrentCell) _onPickCell?.Invoke(picked, _currentCell);
            Close();
        });
        btn.gameObject.SetActive(true);
    }

    private void PlaceOneByOne(RectTransform rt, bool rightSide)
    {
        if (rt == null) return;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(rightSide ? 0f : 1f, 0.5f);
        rt.sizeDelta = new Vector2(cell, cell);
        rt.anchoredPosition = new Vector2((rightSide ? +1 : -1) * gapFromTile, 0f);
    }

    private void Wire()
    {
        _wired = false;

        _canvas = GetComponentInParent<Canvas>(true);
        _root = transform as RectTransform;               // ★ 스크립트는 반드시 DestructPanel에 붙이세요
        _dimmer = FindRT(_root, "Dimmer");
        _anchor = FindRT(_root, "Anchor");
        _leftGrid = FindRT(_root, "LeftGrid");
        _rightGrid = FindRT(_root, "RightGrid");

        // Dimmer 클릭으로 닫기
        if (_dimmer && closeOnBackground)
        {
            var bgBtn = _dimmer.GetComponent<Button>();
            if (bgBtn != null)
            {
                bgBtn.onClick.RemoveAllListeners();
                bgBtn.onClick.AddListener(Close);
            }
        }

        _runtimeButtonPrefab = buttonPrefab != null
            ? buttonPrefab
            : Resources.Load<TowerPlaceButton>("Map/TowerPlaceButton");

        _wired = (_canvas && _root && _anchor && _leftGrid && _rightGrid && _runtimeButtonPrefab);
        if (!_wired)
        {
            Debug.LogError("[DestructUI] Wire failed. Check children names: Dimmer/Anchor/LeftGrid/RightGrid and button prefab.");
        }
    }

    private Camera GetUiCamera()
    {
        // Overlay면 null을 넘겨야 RectTransformUtility 계산이 맞습니다.
        if (_canvas == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
        return _canvas.worldCamera != null ? _canvas.worldCamera : Camera.main;
    }

    private static RectTransform FindRT(Transform root, string name)
    {
        if (!root) return null;
        foreach (RectTransform r in root.GetComponentsInChildren<RectTransform>(true))
            if (r.name == name) return r;
        return null;
    }

    private static void ClearChildren(Transform t)
    {
        if (!t) return;
        for (int i = t.childCount - 1; i >= 0; --i)
            UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
    }
}
