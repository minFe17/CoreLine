using System.Collections.Generic;
using UnityEngine;

public class PathRenderer : MonoBehaviour
{
    [Header("Runner sprite")]
    [SerializeField] private Sprite _arrowSprite;
    [SerializeField] private Vector2 _arrowSize = new Vector2(0.45f, 0.45f);

    [Header("Runners (moving arrows)")]
    [SerializeField] private int _runnerCount = 5;
    [SerializeField] private float _runnerSpeed = 2.0f;     // s-공간(곡률 길이)에서의 속도
    [SerializeField] private float _runnerSpacing = 0.8f;   // 목표 간격 (SetPath 시 갱신)
    [SerializeField] private int _runnerSortingOrder = 4;

    [Header("Auto layout")]
    [SerializeField] private bool _autoFitRunners = false;   // 경로 길이에 따라 개수 자동 조절 여부(권장: false)
    [SerializeField] private float _minScale = 2.0f;
    [SerializeField] private float _margin = 1.0f;

    [Header("Smoothing")]
    [SerializeField] private float _spacingSmoothTime = 0.15f; // 간격 전이 시간
    [SerializeField] private float _headSmoothTime = 0.00f;    // 필요 시 헤드도 보간 (보통 0)

    private readonly List<SpriteRenderer> _runnerPool = new();
    private readonly List<Vector3> _poly = new();
    private readonly List<float> _accLen = new();
    private float _totalLen = 0f;

    // 러너 배치용 파라미터
    private float _headS = 0f;            // 현재 헤드의 호길이 위치
    private float _targetSpacing = 0.8f;  // 목표 간격
    private float _currentSpacing = 0.8f; // 보간 중인 현재 간격
    private float _spacingVel = 0f;       // SmoothDamp용
    private float _headSVel = 0f;         // 헤드 보간용

    void Update()
    {
        if (_poly.Count < 2 || _totalLen <= Mathf.Epsilon)
        {
            for (int i = 0; i < _runnerPool.Count; i++)
                if (_runnerPool[i]) _runnerPool[i].gameObject.SetActive(false);
            return;
        }

        // 간격 보간
        if (!Mathf.Approximately(_currentSpacing, _targetSpacing))
        {
            _currentSpacing = Mathf.SmoothDamp(_currentSpacing, _targetSpacing, ref _spacingVel, Mathf.Max(0.0001f, _spacingSmoothTime));
        }

        // 헤드 위치 적분(속도는 s-공간의 길이/초)
        float headBefore = _headS;
        _headS = Mathf.Repeat(_headS + _runnerSpeed * Time.deltaTime, _totalLen);

        // 필요 시 헤드도 보간(보통 0f 유지)
        if (_headSmoothTime > 0.0001f)
        {
            float target = _headS;
            // 순환 구간 보정
            if (Mathf.Abs(target - headBefore) > _totalLen * 0.5f)
            {
                if (target < headBefore) target += _totalLen;
                else headBefore += _totalLen;
            }
            _headS = Mathf.SmoothDamp(headBefore, target, ref _headSVel, _headSmoothTime);
            _headS = Mathf.Repeat(_headS, _totalLen);
        }

        // 등간격 배치: s_k = headS - k * currentSpacing (모듈로 totalLen)
        for (int k = 0; k < _runnerPool.Count; k++)
        {
            float s = Mathf.Repeat(_headS - k * _currentSpacing, _totalLen);
            PlaceRunner(k, s);
        }
    }

    /// <summary>경로 갱신. 위상(phase) 보존 + 간격 목표치 갱신 + 필요 시 개수 자동 조절</summary>
    public void SetPath(TestMap map, List<Vector2Int> path)
    {
        if (map == null || path == null || path.Count < 2)
        {
            Clear();
            return;
        }

        // 1) 이전 길이/위상 저장
        float oldLen = _totalLen;
        float oldHeadPhase = 0f;
        if (oldLen > Mathf.Epsilon) oldHeadPhase = Mathf.Repeat(_headS, oldLen) / oldLen;

        // 2) 새 폴리라인/accLen/totalLen 계산
        BuildPolylineCache(map, path);

        // 3) 러너 개수/간격 목표 계산
        float newUsableLen = Mathf.Max(0f, _totalLen - 2f * Mathf.Max(0f, _margin));

        if (_autoFitRunners)
        {
            // 자동 배치 모드: 길이에 맞춰 최대한 균등 배치되도록 개수/간격 산정
            ComputeAutoRunnerLayout(newUsableLen);
        }
        else
        {
            // 개수 고정: 길이에 맞춰 목표 간격만 업데이트
            _targetSpacing = (_runnerCount > 1)
                ? (newUsableLen / Mathf.Max(1, _runnerCount - 1))
                : newUsableLen;
            float spriteW = _arrowSize.x > 0f ? _arrowSize.x : 0.4f;
            float minSpacing = spriteW * Mathf.Max(0.8f, _minScale);
            if (_targetSpacing < minSpacing) _targetSpacing = minSpacing;
        }

        // 4) 헤드 위상 보존 (경로가 바뀌어도 같은 phase 유지)
        if (_totalLen > Mathf.Epsilon)
        {
            _headS = Mathf.Repeat(oldHeadPhase * _totalLen, _totalLen);
        }
        else
        {
            _headS = 0f;
        }

        // 5) 풀 준비(개수 증감 반영), 최초 배치
        PrepareRunners();

        // 처음 프레임부터 일정해 보이도록 currentSpacing을 즉시 또는 빠르게 끌고 옴
        if (_spacingSmoothTime <= 0.0001f) _currentSpacing = _targetSpacing;

        for (int k = 0; k < _runnerPool.Count; k++)
        {
            float s = Mathf.Repeat(_headS - k * _currentSpacing, _totalLen);
            PlaceRunner(k, s);
        }
    }

    public void Clear()
    {
        for (int i = 0; i < _runnerPool.Count; i++)
            if (_runnerPool[i]) _runnerPool[i].gameObject.SetActive(false);

        _poly.Clear();
        _accLen.Clear();
        _totalLen = 0f;

        // 상태 유지(다음 경로에서 위상 보존할 필요 없으니 head/spacing은 건드리지 않음)
    }

    // ───────────────────────── 내부 구현 ─────────────────────────

    private void BuildPolylineCache(TestMap map, List<Vector2Int> path)
    {
        _poly.Clear(); _accLen.Clear();
        _accLen.Add(0f);
        _totalLen = 0f;

        Vector3 prev = map.CellToWorld(path[0].x, path[0].y);
        prev.z = 0f;
        _poly.Add(prev);

        for (int i = 1; i < path.Count; i++)
        {
            Vector3 p = map.CellToWorld(path[i].x, path[i].y);
            p.z = 0f;
            _poly.Add(p);

            _totalLen += Vector3.Distance(prev, p);
            _accLen.Add(_totalLen);
            prev = p;
        }
    }

    // 자동 배치 모드일 때만 호출
    private void ComputeAutoRunnerLayout(float usableLen)
    {
        // 기존 로직 응용: 길이에 맞춰 개수/간격 산정
        float spriteW = _arrowSize.x > 0f ? _arrowSize.x : 0.4f;
        float minSpacing = spriteW * Mathf.Max(0.8f, _minScale);
        int maxRunners = usableLen <= 0f ? 1 : Mathf.FloorToInt(usableLen / minSpacing) + 1;

        _runnerCount = Mathf.Clamp(_runnerCount, 1, Mathf.Max(1, maxRunners));
        _targetSpacing = (_runnerCount > 1) ? (usableLen / (_runnerCount - 1)) : usableLen;
        if (_targetSpacing < minSpacing) _targetSpacing = minSpacing;
    }

    private void PrepareRunners()
    {
        // 부족하면 생성
        while (_runnerPool.Count < _runnerCount)
        {
            GameObject go = new GameObject("PathRunner");
            go.transform.SetParent(transform, false);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _arrowSprite;
            sr.sortingOrder = _runnerSortingOrder;

            Vector2 sp = sr.sprite ? (Vector2)sr.sprite.bounds.size : Vector2.one;
            sr.transform.localScale = new Vector3(
                sp.x > 0f ? _arrowSize.x / sp.x : 1f,
                sp.y > 0f ? _arrowSize.y / sp.y : 1f,
                1f
            );

            sr.gameObject.SetActive(false);
            _runnerPool.Add(sr);
        }

        // 남으면 비활성
        for (int i = _runnerCount; i < _runnerPool.Count; i++)
            if (_runnerPool[i]) _runnerPool[i].gameObject.SetActive(false);
    }

    private void PlaceRunner(int k, float s)
    {
        if (k < 0 || k >= _runnerPool.Count) return;
        SpriteRenderer sr = _runnerPool[k];
        if (!sr) return;

        SampleAtDistance(s, out Vector3 pos, out float angleDeg);

        sr.gameObject.SetActive(true);
        sr.transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, 0f, angleDeg));
    }

    private void SampleAtDistance(float s, out Vector3 pos, out float angleDeg)
    {
        int hi = _accLen.Count - 1;
        int lo = 0, mid;
        while (lo < hi)
        {
            mid = (lo + hi) >> 1;
            if (s <= _accLen[mid]) hi = mid;
            else lo = mid + 1;
        }
        int i1 = Mathf.Clamp(lo, 1, _accLen.Count - 1);
        int i0 = i1 - 1;

        float segLen = _accLen[i1] - _accLen[i0];
        float t = segLen > 1e-6f ? Mathf.Clamp01((s - _accLen[i0]) / segLen) : 0f;

        Vector3 a = _poly[i0];
        Vector3 b = _poly[i1];
        pos = Vector3.Lerp(a, b, t);

        Vector2 dir = (b - a);
        angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
    }
}
