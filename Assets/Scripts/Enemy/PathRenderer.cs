using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathRenderer : MonoBehaviour
{
    LineRenderer _line;

    void Awake()
    {
        _line = GetComponent<LineRenderer>();
        _line.useWorldSpace = true;
    }

    public void Clear()
    {
        if (!_line) _line = GetComponent<LineRenderer>();
        _line.positionCount = 0;
    }

    public void SetPath(TestMap map, List<Vector2Int> path)
    {
        if (!_line) _line = GetComponent<LineRenderer>();
        if (map == null || path == null || path.Count == 0)
        {
            Clear(); return;
        }

        _line.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            var rc = path[i]; // (r,c)
            Vector3 p = map.CellToWorld(rc.x, rc.y); // Ç×»ó ¼¿ Áß¾Ó
            _line.SetPosition(i, p);
        }
    }
}
