using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TimerPanelUI : MonoBehaviour
{
    [Header("Refs - Timer Bar")]
    [SerializeField] private RectTransform _barBg;   // BarBg (RectTransform)
    [SerializeField] private Image _barFill;         // BarFill (Image, Filled-Horizontal, Origin=Left)
    [SerializeField] private RectTransform _marker;  // (옵션) Marker. 지금은 멈춤에 사용하지 않음

    [Header("Countdown Label")]
    [SerializeField] private TextMeshProUGUI _countdownText;         // 120,119,... 표시할 Text (BarBg의 자식 추천)
    [SerializeField] private RectTransform _countdownRt;  // 라벨 RectTransform(미지정이면 countdownText의 RT 사용)
    [SerializeField] private float _countdownYOffset = 60f; // 바 위로 띄울 오프셋
    [SerializeField] private float _countdownXPadding = 6f; // 양 끝에서 살짝 안쪽으로 클램프

    [Header("Refs - Toggle Button")]
    [SerializeField] private Button _toggleButton;   // 초록 버튼
    [SerializeField] private RectTransform _arrow;   // 화살표 이미지 (버튼 하위)

    [Header("Progress Settings")]
    [SerializeField] private float _durationSeconds = 120f; // 꽉 차는 데 걸리는 시간(초)
    [SerializeField] private bool _autoStart = true;        // 시작 시 자동 진행

    [Header("Panel Slide")]
    [SerializeField] private float _slideDistance = 200f; // 위/아래로 이동 폭(px)
    [SerializeField] private float _slideDuration = 0.2f; // 슬라이드 시간(초)

    [Header("Start Gate")]
    [Tooltip("플레이어 베이스가 설치된 뒤에만 타이머를 시작합니다.")]
    [SerializeField] private bool _startOnlyWhenBasePlaced = true;


    private RectTransform _panel;
    private Vector2 _panelStartPos;
    private bool _isHidden;
    private bool _sliding;

    private bool _running;
    private float _elapsed;

    // ── 추가: MapManager 이벤트 구독 관리 ──
    private bool _hookedBaseEvent = false;

    private void Awake()
    {
        _panel = (RectTransform)transform;
        _panelStartPos = _panel.anchoredPosition;

        if (_toggleButton)
        {
            _toggleButton.onClick.RemoveAllListeners();
            _toggleButton.onClick.AddListener(OnToggleButton);
        }

        if (_barFill) _barFill.fillAmount = 0f;
        SetCountdown(_durationSeconds);
        PositionCountdownAlongBar(0f);
    }

    private void OnEnable()
    {
        _current = this;
        ResetProgress();

        if (_startOnlyWhenBasePlaced)
        {
            _running = false;                 // 설치 전에는 멈춤
            HookBaseEvent();
            // 이미 설치돼 있으면 즉시 시작
            if (MapManager.Instance != null && MapManager.Instance.HasPlayerBase)
                _running = true;
        }
        else
        {
            if (_autoStart) _running = true;  // 기존 동작 유지
        }
    }

    private void OnDisable()
    {
        if (_current == this) _current = null;
        UnhookBaseEvent();
    }

    private void HookBaseEvent()
    {
        if (_hookedBaseEvent) return;
        if (MapManager.Instance != null)
        {
            MapManager.Instance.OnPlayerBasePlaced += OnBasePlaced;
            _hookedBaseEvent = true;
        }
        else
        {
            StartCoroutine(CoWaitMapAndHook());
        }
    }

    private IEnumerator CoWaitMapAndHook()
    {
        yield return null;
        while (MapManager.Instance == null) yield return null;
        MapManager.Instance.OnPlayerBasePlaced += OnBasePlaced;
        _hookedBaseEvent = true;
    }

    private void UnhookBaseEvent()
    {
        if (!_hookedBaseEvent) return;
        var map = MapManager.Instance;
        if (map != null) map.OnPlayerBasePlaced -= OnBasePlaced;
        _hookedBaseEvent = false;
    }

    private void OnBasePlaced(Vector3Int _)
    {
        _running = true;            // 설치되면 카운트 시작
        // 한 번만 필요하면 구독 해제
        UnhookBaseEvent();
    }


    private void Update()
    {
        if (_running && _barFill && _durationSeconds > 0f)
        {
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _durationSeconds); // 0→1
            _barFill.fillAmount = t;

            // 남은 시간(초) = 올림(Ceil)로 120,119,...0
            float remain = Mathf.Max(0f, _durationSeconds - _elapsed);
            SetCountdown(remain);

            // 라벨을 바 진행 위치에 맞춰 이동
            PositionCountdownAlongBar(t);

            if (t >= 1f) _running = false; // 2분 경과 시 정지
        }
    }
    private void OnToggleButton()
    {
        if (_sliding) return;
        _isHidden = !_isHidden;

        // 화살표 위/아래 반전(세로 뒤집기) : scale.y 토글
        if (_arrow)
        {
            Vector3 arrowDirection = _arrow.localScale;
            arrowDirection.y = _isHidden ? -Mathf.Abs(arrowDirection.y) : Mathf.Abs(arrowDirection.y);
            _arrow.localScale = arrowDirection;
        }

        // 패널 슬라이드
        Vector2 from = _panel.anchoredPosition;
        Vector2 to = _panelStartPos + new Vector2(0f, _isHidden ? _slideDistance : 0f);
        StartCoroutine(CoSlide(from, to, _slideDuration));
    }

    private IEnumerator CoSlide(Vector2 startAnchoredPos, Vector2 targetAnchoredPos, float durationSeconds)
    {
        _sliding = true;
        float elapsedSeconds = 0f;

        while (elapsedSeconds < durationSeconds)
        {
            elapsedSeconds += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsedSeconds / durationSeconds);

            _panel.anchoredPosition = Vector2.Lerp(startAnchoredPos, targetAnchoredPos, progress);
            yield return null;
        }

        _panel.anchoredPosition = targetAnchoredPos;
        _sliding = false;
    }

    // ──────────────────────────────────────────────────────
    // 내부 유틸
    // ──────────────────────────────────────────────────────


    private void SetCountdown(float secondsLeft)
    {
        if (!_countdownText) return;
        int sec = Mathf.CeilToInt(secondsLeft);
        if (sec < 0) sec = 0;
        _countdownText.text = sec.ToString();
    }

    /// <summary>
    /// t(0~1)에 따라 라벨을 BarBg 내부 좌→우로 이동.
    /// pivot이 무엇이든 동작하도록 좌측/우측 로컬X를 계산.
    /// </summary>
    private void PositionCountdownAlongBar(float t01)
    {
        if (!_barBg || !_countdownRt) return;

        // BarBg 좌표계에서 좌/우/윗변 계산
        Rect r = _barBg.rect;
        float xMin = r.xMin + _countdownXPadding;
        float xMax = r.xMax - _countdownXPadding;
        float x = Mathf.Lerp(xMin, xMax, Mathf.Clamp01(t01));

        // y는 바 윗변 + 오프셋 (바 높이에 상관없이 항상 윗쪽)
        float y = r.yMax + _countdownYOffset;

        _countdownRt.anchoredPosition = new Vector2(x, y);
    }


    private void ResetProgress()
    {
        _elapsed = 0f;
        if (_barFill) _barFill.fillAmount = 0f;
        SetCountdown(_durationSeconds);
        PositionCountdownAlongBar(0f);
    }

    // 외부에서 제어용
    public void RestartProgress(float startSeconds = -1f)
    {
        // startSeconds < 0 이면 0초부터
        _elapsed = Mathf.Max(0f, (startSeconds < 0f) ? 0f : (_durationSeconds - startSeconds));
        _running = true;
    }
    public void StopProgress() => _running = false;
    public void ResumeProgress() => _running = true;
    public float GetRemainingSeconds()
    {
        // 남은 시간(초)
        return Mathf.Max(0f, _durationSeconds - _elapsed);
    }

    public bool IsTimeOver()
    {
        // 제한 시간 소진 여부
        return _elapsed >= _durationSeconds;
    }
    // ─────────────────────────────────────────────
    //  - 가장 최근에 Enabled 된 TimerPanelUI를 참조
    // ─────────────────────────────────────────────
    private static TimerPanelUI _current;

    // "전역처럼" 쓰는 간단 래퍼들
    public static bool TryGetRemainingSeconds(out float seconds)
    {
        if (_current != null)
        {
            seconds = _current.GetRemainingSeconds();
            return true;
        }
        seconds = 0f;
        return false;
    }
    public static float RemainingSecondsOrZero => _current ? _current.GetRemainingSeconds() : 0f;

    public static bool IsTimeOverGlobal => _current ? _current.IsTimeOver() : false;

    public static bool IsClickOnBlockButton()
    {
        if (_current == null || _current._toggleButton == null)
            return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        GameObject toggleButtonObj = _current._toggleButton.gameObject;

        foreach (RaycastResult result in raycastResults)
        {
            GameObject hit = result.gameObject;

            if (hit == toggleButtonObj || hit.transform.IsChildOf(toggleButtonObj.transform))
                return true;
        }

        return false;
    }

}
