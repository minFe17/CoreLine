using System;
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
    [SerializeField] private TextMeshProUGUI _countdownText; // 120,119,... 표시할 Text (BarBg의 자식 추천)
    [SerializeField] private RectTransform _countdownRt;     // 라벨 RectTransform(미지정이면 countdownText의 RT 사용)
    [SerializeField] private float _countdownYOffset = 60f;  // 바 위로 띄울 오프셋
    [SerializeField] private float _countdownXPadding = 6f;  // 양 끝에서 살짝 안쪽으로 클램프

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

    // 이벤트: 타이머가 실제로 시작될 때 1회 발화(코스트 해제 등에 사용)
    public event Action ProgressStarted;
    public float TotalDuration => _durationSeconds;

    private RectTransform _panel;
    private Vector2 _panelStartPos;
    private bool _isHidden;
    private bool _sliding;

    private bool _running;
    private float _elapsed;
    private bool _victoryFired;
    private bool _progressStartedFired;

    // MapManager 이벤트 구독 관리
    private bool _hookedBaseEvent;

    // 전역처럼 접근할 수 있게 유지
    private static TimerPanelUI _current;

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

        // _countdownRt가 비어있으면 텍스트의 RectTransform 사용
        if (_countdownRt == null && _countdownText != null)
            _countdownRt = _countdownText.rectTransform;

        // 초기 UI 세팅
        _elapsed = 0f;
        UpdateBarFromElapsed();
    }

    private void OnEnable()
    {
        _current = this;

        _victoryFired = false;
        _progressStartedFired = false;

        // 항상 시각적/내부 상태 리셋(진행은 StartRunning에서만 시작)
        _elapsed = 0f;
        _running = false;
        UpdateBarFromElapsed();

        if (_startOnlyWhenBasePlaced)
        {
            HookBaseEvent();
            // 이미 베이스가 설치되어 있으면 즉시 시작
            if (MapManager.Instance != null && MapManager.Instance.HasPlayerBase)
                StartRunning();
        }
        else
        {
            if (_autoStart) StartRunning();
        }
    }

    private void OnDisable()
    {
        if (_current == this) _current = null;
        UnhookBaseEvent();
    }

    // ───────────────────────────────────────
    // 시작/중지/리셋
    // ───────────────────────────────────────

    private void StartRunning()
    {
        if (_running) return;
        _running = true;

        if (!_progressStartedFired)
        {
            _progressStartedFired = true;
            ProgressStarted?.Invoke(); // 예: CostManager.Instance?.SetEarningEnabled(true);
        }
    }

    public void StopProgress() => _running = false;

    public void ResumeProgress() => StartRunning();

    /// <summary>
    /// 시각적 리셋만 수행(완료 판정/이벤트 발화 없음).
    /// toSeconds: 남은 시간(초)로 세팅. 예: 0 → 게이지 0, 60 → 절반.
    /// </summary>
    public void RestartProgress(float toSeconds)
    {
        _running = false;
        _victoryFired = false;

        float clampedLeft = Mathf.Clamp(toSeconds, 0f, _durationSeconds);
        _elapsed = _durationSeconds - clampedLeft; // 내부는 elapsed 기준
        _elapsed = Mathf.Clamp(_elapsed, 0f, _durationSeconds);

        UpdateBarFromElapsed();
    }

    // ───────────────────────────────────────
    // MapManager 연동(베이스 설치 게이트)
    // ───────────────────────────────────────

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
        StartRunning();
        // 한 번만 필요하면 구독 해제
        UnhookBaseEvent();
    }

    // ───────────────────────────────────────
    // 업데이트 & 완료 처리
    // ───────────────────────────────────────

    private void Update()
    {
        if (_running && _barFill && _durationSeconds > 0f)
        {
            _elapsed += Time.deltaTime;
            if (_elapsed > _durationSeconds) _elapsed = _durationSeconds;

            UpdateBarFromElapsed();

            if (_elapsed >= _durationSeconds)
            {
                _running = false;
                OnTimerFinished();
            }
        }
    }

    private void OnTimerFinished()
    {
        if (_victoryFired) return;
        _victoryFired = true;

        var mgr = NormalStageManager.Instance;
        if (mgr == null) return;

        var stage = mgr.SelectedStage;
        var snap = ConditionControl.BuildFor(stage);
        mgr.CompleteStageSuccess(snap); // 승리 한 번만!
    }

    // ───────────────────────────────────────
    // UI: 패널 슬라이드/토글
    // ───────────────────────────────────────

    private void OnToggleButton()
    {
        if (_sliding) return;
        _isHidden = !_isHidden;

        // 화살표 위/아래 반전(세로 뒤집기)
        if (_arrow)
        {
            Vector3 s = _arrow.localScale;
            s.y = _isHidden ? -Mathf.Abs(s.y) : Mathf.Abs(s.y);
            _arrow.localScale = s;
        }

        Vector2 from = _panel.anchoredPosition;
        Vector2 to = _panelStartPos + new Vector2(0f, _isHidden ? _slideDistance : 0f);
        StartCoroutine(CoSlide(from, to, _slideDuration));
    }
    public void SetDuration(float seconds, bool restartToFull = true)
    {
        _durationSeconds = Mathf.Max(1f, seconds);

        if (restartToFull)
        {
            // 남은 시간을 총시간으로 리셋(게이지 0부터 시작, 진행은 멈춘 상태)
            RestartProgress(_durationSeconds);
            StopProgress();
        }
    }
    private IEnumerator CoSlide(Vector2 startAnchoredPos, Vector2 targetAnchoredPos, float durationSeconds)
    {
        _sliding = true;
        float elapsedSeconds = 0f;

        while (elapsedSeconds < durationSeconds)
        {
            elapsedSeconds += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(elapsedSeconds / durationSeconds);

            _panel.anchoredPosition = Vector2.Lerp(startAnchoredPos, targetAnchoredPos, p);
            yield return null;
        }

        _panel.anchoredPosition = targetAnchoredPos;
        _sliding = false;
    }

    // ───────────────────────────────────────
    // 내부 유틸 (게이지/라벨)
    // ───────────────────────────────────────

    private void UpdateBarFromElapsed()
    {
        float t = (_durationSeconds > 0f) ? Mathf.Clamp01(_elapsed / _durationSeconds) : 0f;
        if (_barFill) _barFill.fillAmount = t;

        float remain = Mathf.Max(0f, _durationSeconds - _elapsed);
        SetCountdown(remain);
        PositionCountdownAlongBar(t);
    }

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

        // y는 바 윗변 + 오프셋
        float y = r.yMax + _countdownYOffset;

        _countdownRt.anchoredPosition = new Vector2(x, y);
    }

    // ───────────────────────────────────────
    // 외부 접근 래퍼
    // ───────────────────────────────────────

    public float GetRemainingSeconds()
    {
        return Mathf.Max(0f, _durationSeconds - _elapsed);
    }

    public bool IsTimeOver()
    {
        return _elapsed >= _durationSeconds;
    }

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
        if (_current == null || _current._toggleButton == null) return false;
        if (EventSystem.current == null) return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        GameObject toggleGO = _current._toggleButton.gameObject;

        for (int i = 0; i < results.Count; i++)
        {
            GameObject hit = results[i].gameObject;
            if (hit == toggleGO || hit.transform.IsChildOf(toggleGO.transform))
                return true;
        }
        return false;
    }
}
