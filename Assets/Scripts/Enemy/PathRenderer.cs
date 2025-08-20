using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathRenderer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TestMap map;

    [Header("Style")]
    [SerializeField] private bool visible = true;

    private LineRenderer line;

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.enabled = visible;
    }

    public void SetMap(TestMap m) => map = m;

    public void Clear()
    {
        if (!line) line = GetComponent<LineRenderer>();
        line.positionCount = 0;
    }

    
    public void SetPath(List<Vector2Int> path)
    {
        if (!line) line = GetComponent<LineRenderer>();

        if (!visible || map == null || path == null || path.Count == 0)
        {
            Clear();
            return;
        }

        line.positionCount = path.Count;

        
        for (int i = 0; i < path.Count; i++)
        {
            var cell = path[i];
            var p = map.CellToWorld(cell.x, cell.y);
            line.SetPosition(i, p);
        }
    }

    public void SetVisible(bool v)
    {
        visible = v;
        if (!line) line = GetComponent<LineRenderer>();
        line.enabled = visible;
    }

    public void SetWidth(float start, float end)
    {
        if (!line) line = GetComponent<LineRenderer>();
        line.startWidth = start;
        line.endWidth = end;
    }

    public void SetColor(Color start, Color end)
    {
        if (!line) line = GetComponent<LineRenderer>();
        line.startColor = start;
        line.endColor = end;
    }
}
