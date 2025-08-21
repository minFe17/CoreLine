using UnityEngine;

public class TestMap : MonoBehaviour
{
    [SerializeField] private int width = 20;
    [SerializeField] private int height = 12;
    [SerializeField] private float cellSize = 1.0f;
    [SerializeField] private Vector3 origin = Vector3.zero;
    public Vector3 Origin { get => origin; set => origin = value; }

    [SerializeField] private bool generateRandomObstacles = true;
    [Range(0f, 1f)] public float ObstacleRate = 0.18f;

    public float CellSize => cellSize;
    public bool[,] Walkable;

    void Awake()
    {
        Walkable = new bool[height, width];
        for (int r = 0; r < height; r++)
            for (int c = 0; c < width; c++)
                Walkable[r, c] = true;

        if (generateRandomObstacles)
        {
            var rand = new System.Random();
            for (int r = 0; r < height; r++)
                for (int c = 0; c < width; c++)
                    if (rand.NextDouble() < ObstacleRate)
                        Walkable[r, c] = false;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            wp.z = 0f;
            Vector2Int point = WorldToCell(wp);
            
            Walkable[point.x, point.y] = (Walkable[point.x, point.y]) ? false : true;
        }
    }

    public bool InBounds(int r, int c) => r >= 0 && c >= 0 && r < height && c < width;
    public bool IsWalkable(int r, int c) => InBounds(r, c) && Walkable[r, c];

    public Vector3 CellToWorld(int r, int c)
        => origin + new Vector3((c + 0.5f) * cellSize, (r + 0.5f) * cellSize, 0f);

    public Vector2Int WorldToCell(Vector3 world)
    {
        Vector3 local = world - origin;
        int c = Mathf.FloorToInt(local.x / cellSize);
        int r = Mathf.FloorToInt(local.y / cellSize);
        r = Mathf.Clamp(r, 0, height - 1);
        c = Mathf.Clamp(c, 0, width - 1);
        return new Vector2Int(r, c);
    }
}
