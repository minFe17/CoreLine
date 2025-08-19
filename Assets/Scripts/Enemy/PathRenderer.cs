using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PathRenderer : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private TestMap map;               
    [SerializeField] private LineRenderer line;     

    [Header("Gizmos (Scene View)")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmoLineColor = new Color(1f, 1f, 0f, 0.95f);
    [SerializeField] private Color gizmoNodeColor = new Color(0.2f, 1f, 0.2f, 0.9f);
    [SerializeField] private float nodeRadius = 0.08f;

    [Header("Runtime Line (Game View)")]
    [SerializeField] private bool drawRuntimeLine = true;

    List<Vector2Int> _path;

    void Awake()
    {
        if (!map) map = Object.FindAnyObjectByType<TestMap>();
        if (!line) line = GetComponent<LineRenderer>();
        if (line) line.positionCount = 0;
    }

    public void SetPath(List<Vector2Int> path)
    {
        _path = path;

        if (!drawRuntimeLine || !line)
        {
            if (line) line.positionCount = 0;
            return;
        }

        if (_path == null || _path.Count == 0)
        {
            line.positionCount = 0;
            return;
        }

        // LineRenderer에 월드 좌표로 세팅
        line.positionCount = _path.Count;
        for (int i = 0; i < _path.Count; i++)
        {
            var p = _path[i];
            Vector3 center = map.CellToWorld(p.x, p.y) + new Vector3(map.CellToWorld(0, 0).x - map.Origin.x, 0f, 0f);
            center = CellCenter(p);
            line.SetPosition(i, center);
        }
    }

    public void ClearPath()
    {
        _path = null;
        if (line) line.positionCount = 0;
    }

    Vector3 CellCenter(Vector2Int cell)
    {
        // 타일 중앙으로 살짝 올림
        return map.CellToWorld(cell.x, cell.y) + new Vector3(map.CellSize * 0.5f, map.CellSize * 0.5f, 0f);
    }

    void OnDrawGizmos()
    {
        if (!drawGizmos || _path == null || _path.Count < 2 || map == null) return;

        Gizmos.color = gizmoLineColor;
        for (int i = 0; i < _path.Count - 1; i++)
        {
            var a = CellCenter(_path[i]);
            var b = CellCenter(_path[i + 1]);
            Gizmos.DrawLine(a, b);
        }

        Gizmos.color = gizmoNodeColor;
        foreach (var p in _path)
            Gizmos.DrawSphere(CellCenter(p), nodeRadius);
    }
}
