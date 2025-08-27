using System;
using UnityEngine;
using UnityEngine.UI;

public class DestructUI : MonoBehaviour
{
    private Canvas _canvas;
    private RectTransform _root;
    private RectTransform _dimmer;
    private RectTransform _anchor;
    private RectTransform _leftBtn;
    private RectTransform _rightBtn;
    private Button _leftButton;
    private Button _rightButton;

    [SerializeField] private float gapFromTile = 16f;

    private Action<TowerOption, Vector3Int> _onPickCell;
    private Vector3Int _currentCell;
    private bool _wired;

    void Awake()
    {
        Wire();
        if (_root) _root.gameObject.SetActive(false);
    }

    void Wire()
    {
        _canvas = GetComponentInParent<Canvas>(true);
        _root = transform as RectTransform;

        // 이름으로 자동 탐색 (하위 오브젝트가 정확히 있어야 함)
        _dimmer = FindRT(_root, "Dimmer");
        _anchor = FindRT(_root, "Anchor");
        _leftBtn = FindRT(_root, "CancelButton");
        _rightBtn = FindRT(_root, "DestroyButton");

        _leftButton = _leftBtn?.GetComponent<Button>();
        _rightButton = _rightBtn?.GetComponent<Button>();

        // Dimmer 닫기
        if (_dimmer)
        {
            var bgBtn = _dimmer.GetComponent<Button>();
            if (bgBtn != null)
            {
                bgBtn.onClick.RemoveAllListeners();
                bgBtn.onClick.AddListener(Close);
            }
        }

        _wired = (_root && _anchor && _leftBtn && _rightBtn && _leftButton && _rightButton);
        if (!_wired) Debug.LogError("[DestructUI] Wire 실패. 자식 이름 확인 필요: Dimmer, Anchor, LeftBtn, RightBtn");
    }

    public void OpenAtCell(Vector3Int cell, Action<TowerOption, Vector3Int> onPickCell)
    {
        if (!_wired) Wire();
        if (!_wired) return;

        _onPickCell = onPickCell;
        _currentCell = cell;

        // 타일 중심을 화면→로컬 변환
        var map = MapManager.Instance;
        Vector3 world = map && map.IsReady ? map.CellCenterWorld(cell) : (Vector3)cell;
        Camera uiCam = (_canvas == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            ? null : (_canvas.worldCamera != null ? _canvas.worldCamera : Camera.main);

        Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCam ?? Camera.main, world);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screen, uiCam, out var local);
        _anchor.anchoredPosition = local;

        // 좌/우 버튼 위치 배치
        PlaceBtn(_leftBtn, false);
        PlaceBtn(_rightBtn, true);

        // 콜백 연결
        _leftButton.onClick.RemoveAllListeners();
        _leftButton.onClick.AddListener(() => { _onPickCell?.Invoke(new TowerOption("cancel", null, null, 0), _currentCell); Close(); });

        _rightButton.onClick.RemoveAllListeners();
        _rightButton.onClick.AddListener(() => { _onPickCell?.Invoke(new TowerOption("destroy", null, null, 0), _currentCell); Close(); });

        _root.gameObject.SetActive(true);
    }

    public void Close()
    {
        _onPickCell = null;
        if (_root) _root.gameObject.SetActive(false);
    }

    // ───────── 유틸 ─────────
    private void PlaceBtn(RectTransform rt, bool rightSide)
    {
        if (rt == null) return;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(rightSide ? 0f : 1f, 0.5f);
        rt.anchoredPosition = new Vector2((rightSide ? +1 : -1) * gapFromTile, 0f);
    }

    private static RectTransform FindRT(Transform root, string name)
    {
        if (!root) return null;
        foreach (RectTransform r in root.GetComponentsInChildren<RectTransform>(true))
            if (r.name == name) return r;
        return null;
    }
}
