using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    [SerializeField] private TestMap map;             
    
    [SerializeField] private bool drawGridLines = true;
    [SerializeField] private bool fillCells = true;

    [SerializeField] private Color gridColor = new Color(1f, 1f, 1f, 0.25f);
    [SerializeField] private Color walkableColor = new Color(0f, 0.7f, 1f, 0.15f);
    [SerializeField] private Color obstacleColor = new Color(1f, 0.15f, 0.15f, 0.35f);

    void OnDrawGizmos()
    {
        if (!map) return;

        int width = GetPrivate(map, "width", 20);
        int height = GetPrivate(map, "height", 12);
        float cell = GetPrivate(map, "cellSize", 1f);
        Vector3 origin = GetPrivate(map, "origin", Vector3.zero);

        if (drawGridLines)
        {
            Gizmos.color = gridColor;
            for (int r = 0; r <= height; r++)
            {
                Vector3 a = origin + new Vector3(0, r * cell, 0);
                Vector3 b = origin + new Vector3(width * cell, r * cell, 0);
                Gizmos.DrawLine(a, b);
            }
            for (int c = 0; c <= width; c++)
            {
                Vector3 a = origin + new Vector3(c * cell, 0, 0);
                Vector3 b = origin + new Vector3(c * cell, height * cell, 0);
                Gizmos.DrawLine(a, b);
            }
        }

        if (fillCells && map.Walkable != null &&
            map.Walkable.GetLength(0) == height && map.Walkable.GetLength(1) == width)
        {
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    Vector3 center = origin + new Vector3((c + 0.5f) * cell, (r + 0.5f) * cell, 0f);
                    Gizmos.color = map.Walkable[r, c] ? walkableColor : obstacleColor;
                    Gizmos.DrawCube(center, new Vector3(cell * 0.98f, cell * 0.98f, 0.01f));
                }
            }
        }
    }

    T GetPrivate<T>(object obj, string field, T fallback)
    {
        var f = obj.GetType().GetField(field, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return f != null ? (T)f.GetValue(obj) : fallback;
    }
}
