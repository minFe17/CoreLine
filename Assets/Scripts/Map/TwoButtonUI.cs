using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TwoButtonUI : MonoBehaviour
{
    private Canvas _canvas;
    private RectTransform _root;
    private RectTransform _dimmer;
    private RectTransform _anchor;

    private RectTransform _leftRT;
    private RectTransform _rightRT;
    private Button _leftBtn;
    private Button _rightBtn;
    private TMP_Text _leftTMP;
    private TMP_Text _rightTMP;


    [SerializeField] private float gapFromTile = 16f;

    private Action<string, object> _onPick;
    private object _payload;
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

        _dimmer = FindRT(_root, "Dimmer");
        _anchor = FindRT(_root, "Anchor");
        _leftRT = FindRT(_root, "CancelButton");
        _rightRT = FindRT(_root, "ActiveButton");

        _leftBtn = _leftRT ? _leftRT.GetComponent<Button>() : null;
        _rightBtn = _rightRT ? _rightRT.GetComponent<Button>() : null;

        _leftTMP = _leftRT ? _leftRT.GetComponentInChildren<TMP_Text>(true) : null;
        _rightTMP = _rightRT ? _rightRT.GetComponentInChildren<TMP_Text>(true) : null;

        if (_dimmer)
        {
            var bgBtn = _dimmer.GetComponent<Button>();
            if (bgBtn != null)
            {
                bgBtn.onClick.RemoveAllListeners();
                bgBtn.onClick.AddListener(Close);
            }
        }

        _wired = (_root && _anchor && _leftBtn && _rightBtn && _leftTMP && _rightTMP);
        if (!_wired) Debug.LogError("[TwoButtonPanelUI] Wire 실패. 구조 확인 필요");
    }

    /// <summary>
    /// 셀 기준으로 열기
    /// </summary>
    public void OpenAtCell(Vector3Int cell, string rightLabel, Action<string, object> onPick)
    {
        var map = MapManager.Instance;
        Vector3 world = map && map.IsReady ? map.CellCenterWorld(cell) : (Vector3)cell;
        OpenInternal(world, rightLabel, onPick, payload: cell);
    }

    /// <summary>
    /// ObjectTile 기준으로 열기
    /// </summary>
    public void OpenAtObject(ObjectTile target, string rightLabel, Action<string, object> onPick)
    {
        var map = MapManager.Instance;
        Vector3 world = target.transform.position;
        if (map && map.IsReady) world = map.CellCenterWorld(map.WorldToCell(world));
        OpenInternal(world, rightLabel, onPick, payload: target);
    }

    void OpenInternal(Vector3 world, string rightLabel, Action<string, object> onPick, object payload)
    {
        if (!_wired) Wire();
        if (!_wired) return;

        _onPick = onPick;
        _payload = payload;

        _root.SetAsLastSibling();

        // 좌표 변환
        var canvasRT = (_canvas ? _canvas.transform as RectTransform : _root);
        Camera uiCam = (_canvas == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            ? null : (_canvas.worldCamera != null ? _canvas.worldCamera : Camera.main);

        Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCam ?? Camera.main, world);
        Vector2 local;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRT, screen, uiCam, out local))
            _anchor.anchoredPosition = local;
        else
            _anchor.anchoredPosition = Vector2.zero;

        // 버튼 배치
        PlaceBtn(_leftRT, false);
        PlaceBtn(_rightRT, true);

        // 텍스트 세팅
        if (_leftTMP) _leftTMP.text = "취소";
        if (_rightTMP) _rightTMP.text = rightLabel;

        // 콜백
        _leftBtn.onClick.RemoveAllListeners();
        _leftBtn.onClick.AddListener(Close);

        _rightBtn.onClick.RemoveAllListeners();
        _rightBtn.onClick.AddListener(() =>
        {
            _onPick?.Invoke("right", _payload);
            Close();
        });

        _root.gameObject.SetActive(true);
    }

    public void Close()
    {
        _onPick = null;
        _payload = null;
        if (_root) _root.gameObject.SetActive(false);
    }

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
