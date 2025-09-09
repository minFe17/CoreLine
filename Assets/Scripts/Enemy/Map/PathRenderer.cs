using System.Collections.Generic;
using UnityEngine;

public class PathRenderer : MonoBehaviour
{
    [Header("Runner sprite")]
    [SerializeField] private Sprite _arrowSprite;
    [SerializeField] private Vector2 _arrowSize = new Vector2(0.45f, 0.45f);

    [Header("Runners (auto-fit, moving arrows)")]
    [SerializeField, Tooltip("목표 간격(고정 의도). 경로가 바뀌어도 이 간격에 가깝게 유지합니다.")]
    private float _preferredSpacing = 2.0f;
    [SerializeField] private float _runnerSpeed = 2.0f;     // s-공간(호길이) 속도
    [SerializeField] private int _runnerSortingOrder = 4;

    [Header("Auto-fit constraints")]
    [SerializeField, Tooltip("스프라이트 실측 폭에 곱하여 최소 간격 하한선을 잡습니다. (minSpacing = ArrowSize.x * max(0.8, MinScale))")]
    private float _minScale = 2.0f;
    [SerializeField, Tooltip("경로 양 끝에서 제외할 길이(총 2*Margin이 빠짐). 큰 빈칸을 만들고 싶지 않다면 0을 권장.")]
    private float _margin = 0.0f;

    [Header("Smoothing")]
    [SerializeField, Tooltip("간격이 바뀔 때 전이 시간. 0이면 즉시 반영.")]
    private float _spacingSmoothTime = 0.15f;
    [SerializeField, Tooltip("헤드 움직임도 보간하고 싶을 때(보통 0).")]
    private float _headSmoothTime = 0.00f;

    [Header("Corner Rounding (Fillet)")]
    [SerializeField, Tooltip("코너 라운딩 반지름(0이면 기존 ‘각지게’ 회전).")]
    private float _cornerRadius = 0.5f;
    [SerializeField] private float _cornerMinAngleDeg = 5f;       // 이보다 작은 코너는 필렛 생략
    [SerializeField, Tooltip("인접 직선 길이 대비 한쪽 트림 최대 비율(안전장치)")]
    private float _maxTrimRatio = 0.4f;

    // ───────────────────────── 내부 상태 ─────────────────────────
    private readonly List<SpriteRenderer> _runnerPool = new();

    // 원래 폴리라인(디버그/유지)
    private readonly List<Vector3> _poly = new();

    // 세그먼트(직선/원호) 목록과 누적 길이
    private readonly List<Seg> _segs = new();
    private readonly List<float> _accLen = new();
    private float _totalLen = 0f;

    // 자동 산정된 러너 개수(정보용)
    [SerializeField, Tooltip("Auto-fit로 자동 계산된 러너 개수(정보용).")]
    private int _runnerCount = 1;

    // 러너 배치용 파라미터
    private float _headS = 0f;            // 현재 헤드의 호길이 위치
    private float _targetSpacing = 0.8f;  // 균등 분배 결과(usableLen/(count-1))
    private float _currentSpacing = 0.8f; // 보간 중인 현재 간격
    private float _spacingVel = 0f;       // SmoothDamp용
    private float _headSVel = 0f;         // 헤드 보간용

    void Update()
    {
        if (_segs.Count == 0 || _totalLen <= Mathf.Epsilon)
        {
            for (int i = 0; i < _runnerPool.Count; i++)
                if (_runnerPool[i]) _runnerPool[i].gameObject.SetActive(false);
            return;
        }

        // 간격 보간
        if (!Mathf.Approximately(_currentSpacing, _targetSpacing))
        {
            _currentSpacing = Mathf.SmoothDamp(
                _currentSpacing,
                _targetSpacing,
                ref _spacingVel,
                Mathf.Max(0.0001f, _spacingSmoothTime)
            );
        }

        // 헤드 위치 적분(속도는 s-공간의 길이/초)
        float headBefore = _headS;
        _headS = Mathf.Repeat(_headS + _runnerSpeed * Time.deltaTime, _totalLen);

        // 필요 시 헤드도 보간
        if (_headSmoothTime > 0.0001f)
        {
            float target = _headS;
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

    /// <summary>
    /// 경로 갱신. Auto-fit을 항상 사용: 원하는 간격(_preferredSpacing)을 기준으로
    /// 러너 수를 자동 산정하고, 전체 길이에 균등 분배된 targetSpacing을 설정.
    /// </summary>
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

        // 2) 새 세그먼트(필렛 포함) 구축
        BuildSegmentsWithFillets(map, path);

        // 3) usable 길이
        float usableLen = Mathf.Max(0f, _totalLen - 2f * Mathf.Max(0f, _margin));

        // 4) Auto-fit: 간격 고정 의도 유지 + 개수 자동
        float spriteW = _arrowSize.x > 0f ? _arrowSize.x : 0.4f;
        float minSpacing = spriteW * Mathf.Max(0.8f, _minScale);
        float sDesired = Mathf.Max(minSpacing, _preferredSpacing);

        int count = (usableLen <= 0f)
            ? 1
            : Mathf.Max(1, Mathf.RoundToInt(usableLen / sDesired) + 1);

        _runnerCount = count;
        _targetSpacing = (count > 1) ? (usableLen / (count - 1)) : usableLen;

        // 5) 위상 보존
        _headS = (_totalLen > Mathf.Epsilon)
            ? Mathf.Repeat(oldHeadPhase * _totalLen, _totalLen)
            : 0f;

        // 6) 풀 준비 & 초기 배치
        PrepareRunners();

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
        _segs.Clear();
        _accLen.Clear();
        _totalLen = 0f;
    }

    // ───────────────────────── 세그먼트/샘플러 ─────────────────────────

    private enum SegType { Line, Arc }

    private struct Seg
    {
        public SegType type;

        // Line
        public Vector3 a, b;

        // Arc (원호)
        public Vector3 center;
        public float radius;
        public float startDeg;     // 0~360
        public float sweepDeg;     // 부호 포함(양: CCW, 음: CW)
        public float length;       // 미리 저장(성능)
    }

    private void BuildSegmentsWithFillets(TestMap map, List<Vector2Int> path)
    {
        _poly.Clear();
        _segs.Clear();
        _accLen.Clear();
        _totalLen = 0f;
        _accLen.Add(0f);

        // 원래 폴리라인(월드 좌표)
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 p = map.CellToWorld(path[i].x, path[i].y);
            p.z = 0f;
            _poly.Add(p);
        }

        if (_poly.Count < 2)
            return;

        const float EPS = 1e-5f;
        float minAngleRad = Mathf.Deg2Rad * Mathf.Max(0f, _cornerMinAngleDeg);
        float rBase = Mathf.Max(0f, _cornerRadius);

        Vector3 last = _poly[0];

        for (int i = 1; i < _poly.Count - 1; i++)
        {
            Vector3 a = _poly[i - 1];
            Vector3 b = _poly[i];
            Vector3 c = _poly[i + 1];

            Vector3 v1 = (b - a);
            Vector3 v2 = (c - b);

            float L1 = v1.magnitude;
            float L2 = v2.magnitude;

            if (L1 <= EPS || L2 <= EPS)
            {
                AddLine(last, b);
                last = b;
                continue;
            }

            Vector3 d1 = v1 / L1; // a→b
            Vector3 d2 = v2 / L2; // b→c

            float phi = Mathf.Atan2(CrossZ(d1, d2), Vector3.Dot(d1, d2));
            float theta = Mathf.Abs(phi);

            if (theta < minAngleRad || rBase <= EPS)
            {
                AddLine(last, b);
                last = b;
                continue;
            }

            // 이상적인 트림 길이
            float r = rBase;
            float tIdeal = r * Mathf.Tan(theta * 0.5f);

            float tMaxByLen = Mathf.Min(L1, L2) - EPS;
            float tMaxByRatio = Mathf.Min(L1, L2) * Mathf.Clamp01(_maxTrimRatio);
            float tMax = Mathf.Max(EPS, Mathf.Min(tMaxByLen, Mathf.Max(EPS, tMaxByRatio)));

            if (tIdeal > tMax)
            {
                float newR = tMax / Mathf.Max(EPS, Mathf.Tan(theta * 0.5f));
                if (newR <= EPS)
                {
                    AddLine(last, b);
                    last = b;
                    continue;
                }
                r = newR;
                tIdeal = Mathf.Min(tIdeal, tMax);
            }

            Vector3 p1 = b - d1 * tIdeal; // 진입 직선의 끝점
            Vector3 p2 = b + d2 * tIdeal; // 이탈 직선의 시작점

            AddLine(last, p1);

            // 중심(내각 이등분)
            Vector3 u1 = -d1; // b 기준 진입 방향
            Vector3 u2 = d2; // b 기준 이탈 방향
            Vector3 bis = (u1 + u2);
            float bisMag = bis.magnitude;

            if (bisMag <= EPS)
            {
                AddLine(p1, p2);
                last = p2;
                continue;
            }

            Vector3 dirToCenter = bis / bisMag;
            float distBC = r / Mathf.Sin(theta * 0.5f);
            Vector3 center = b + dirToCenter * distBC;

            Vector2 vStart = (p1 - center);
            Vector2 vEnd = (p2 - center);

            float startDeg = Mathf.Atan2(vStart.y, vStart.x) * Mathf.Rad2Deg;
            float endDeg = Mathf.Atan2(vEnd.y, vEnd.x) * Mathf.Rad2Deg;

            float sweepDeg = Mathf.DeltaAngle(startDeg, endDeg); // [-180,180]
            if (Mathf.Sign(sweepDeg) != Mathf.Sign(phi))
                sweepDeg = -sweepDeg;

            AddArc(center, r, startDeg, sweepDeg);

            last = p2;
        }

        AddLine(last, _poly[_poly.Count - 1]);

        // 누적 길이
        _accLen.Clear();
        _accLen.Add(0f);
        _totalLen = 0f;
        for (int i = 0; i < _segs.Count; i++)
        {
            _totalLen += _segs[i].length;
            _accLen.Add(_totalLen);
        }
    }

    private void AddLine(Vector3 a, Vector3 b)
    {
        const float EPS = 1e-6f;
        float len = (b - a).magnitude;
        if (len <= EPS) return;

        _segs.Add(new Seg
        {
            type = SegType.Line,
            a = a,
            b = b,
            length = len
        });
    }

    private void AddArc(Vector3 center, float radius, float startDeg, float sweepDeg)
    {
        float len = Mathf.Abs(sweepDeg) * Mathf.Deg2Rad * Mathf.Max(0f, radius);
        if (len <= 1e-6f) return;

        _segs.Add(new Seg
        {
            type = SegType.Arc,
            center = center,
            radius = radius,
            startDeg = Normalize360(startDeg),
            sweepDeg = sweepDeg,
            length = len
        });
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
        pos = Vector3.zero;
        angleDeg = 0f;
        if (_segs.Count == 0) return;

        // 이진 탐색
        int lo = 0, hi = _accLen.Count - 1;
        while (lo < hi)
        {
            int mid = (lo + hi) >> 1;
            if (s <= _accLen[mid]) hi = mid;
            else lo = mid + 1;
        }
        int idx = Mathf.Clamp(lo - 1, 0, _segs.Count - 1);

        Seg seg = _segs[idx];
        float s0 = _accLen[idx];
        float u = Mathf.Clamp01((s - s0) / Mathf.Max(1e-6f, seg.length));

        if (seg.type == SegType.Line)
        {
            pos = Vector3.Lerp(seg.a, seg.b, u);
            Vector2 dir = (seg.b - seg.a);
            angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        }
        else // Arc
        {
            float ang = seg.startDeg + seg.sweepDeg * u;
            float rad = ang * Mathf.Deg2Rad;

            Vector3 rvec = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * seg.radius;
            pos = seg.center + rvec;

            // 접선 방향: CCW(양수)면 +90°, CW(음수)면 -90°
            angleDeg = ang + (seg.sweepDeg >= 0f ? 90f : -90f);
        }
    }

    // ───────────────────────── 러너 풀/보조 ─────────────────────────

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

    private static float CrossZ(Vector3 a, Vector3 b) => a.x * b.y - a.y * b.x;
    private static float Normalize360(float deg)
    {
        deg %= 360f;
        if (deg < 0f) deg += 360f;
        return deg;
    }
}