using System.Collections.Generic;
using UnityEngine;

public class PathRenderer : MonoBehaviour
{
    [Header("Runner sprite")]
    [SerializeField] private Sprite _arrowSprite;
    [SerializeField] private Vector2 _arrowSize = new Vector2(0.45f, 0.45f);

    [Header("Runners (moving arrows)")]
    [SerializeField] private int _runnerCount = 5;          
    [SerializeField] private float _runnerSpeed = 2.0f;
    [SerializeField] private float _runnerSpacing = 0.8f;    
    [SerializeField] private int _runnerSortingOrder = 4;

    [Header("Auto layout")]
    [SerializeField] private float _minScale = 2.0f;  
    [SerializeField] private float _margin = 1.0f;

    private readonly List<SpriteRenderer> _runnerPool = new();
    private readonly List<Vector3> _poly = new();   
    private readonly List<float> _accLen = new();   
    private float _totalLen = 0f;
    private float _headS = 0f;

    void Update()
    {
        if (_poly.Count < 2 || _totalLen <= Mathf.Epsilon)
        {
            for (int i = 0; i < _runnerPool.Count; i++)
                if (_runnerPool[i]) _runnerPool[i].gameObject.SetActive(false);
            return;
        }

        _headS = Mathf.Repeat(_headS + _runnerSpeed * Time.deltaTime, _totalLen);

        for (int k = 0; k < _runnerPool.Count; k++)
        {
            float s = Mathf.Repeat(_headS - k * _runnerSpacing, _totalLen);
            PlaceRunner(k, s);
        }
    }

    public void SetPath(TestMap map, List<Vector2Int> path)
    {
        if (map == null || path == null || path.Count < 2)
        {
            Clear();
            return;
        }

        BuildPolylineCache(map, path);
        RecomputeRunnerLayout();   
        PrepareRunners();          

        _headS = 0f;             
        for (int k = 0; k < _runnerPool.Count; k++)
        {
            float s = Mathf.Repeat(-k * _runnerSpacing, _totalLen);
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
        _headS = 0f;
    }

   
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

    private void PrepareRunners()
    {
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

        for (int i = _runnerCount; i < _runnerPool.Count; i++)
            if (_runnerPool[i]) _runnerPool[i].gameObject.SetActive(false);
    }

    private void PlaceRunner(int k, float s)
    {
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

    private void RecomputeRunnerLayout()
    {
        if (_totalLen <= Mathf.Epsilon)
        {
            _runnerCount = 0;
            return;
        }

        float spriteW = _arrowSize.x > 0f ? _arrowSize.x : 0.4f;
        float minSpacing = spriteW * Mathf.Max(0.8f, _minScale);

        float usableLen = Mathf.Max(0f, _totalLen - 2f * Mathf.Max(0f, _margin));
        int maxRunners = usableLen <= 0f ? 1 : Mathf.FloorToInt(usableLen / minSpacing) + 1;

        _runnerCount = Mathf.Clamp(_runnerCount, 1, Mathf.Max(1, maxRunners));
        _runnerSpacing = (_runnerCount > 1) ? (usableLen / (_runnerCount - 1)) : usableLen;
        if (_runnerSpacing < minSpacing) _runnerSpacing = minSpacing;
    }
}
