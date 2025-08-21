using System;
using UnityEngine;

public class TestMap : MonoBehaviour
{
    [Flags]
    public enum CellFlags
    {
        None = 0,
        Wall = 1 << 0,  // 파괴 불가 벽
        Destructible = 1 << 1,  // 파괴 가능 벽
        Tower = 1 << 2,  // 타워/점유
        // 추후 확장: Slow=1<<3, Poison=1<<4 ...
    }

    [Header("Grid Size")]
    [SerializeField] private int width = 20;   // 열 수 (c, x)
    [SerializeField] private int height = 12;  // 행 수 (r, y)

    [Header("Cell World Transform")]
    [SerializeField] private float cellSize = 1.0f;
    [SerializeField] private Vector3 origin = Vector3.zero;

    public int Width => width;
    public int Height => height;
    public float CellSize => cellSize;
    public Vector3 Origin { get => origin; set => origin = value; }

    [Header("Random Obstacles (optional)")]
    [SerializeField] private bool generateRandomObstacles = true;
    [Range(0f, 1f)] public float obstacleRate = 0.18f;
    [SerializeField] private float destructibleRatio = 0.35f; // 장애물 중 파괴가능 비율

    // 셀 상태
    public CellFlags[,] cells;

    // 셀 변경 알림 (r, c)
    public event Action<int, int> OnCellChanged;

    void Awake()
    {
        cells = new CellFlags[height, width];

        if (generateRandomObstacles)
        {
            var rand = new System.Random();
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    if (rand.NextDouble() < obstacleRate)
                    {
                        bool makeDes = rand.NextDouble() < destructibleRatio;
                        cells[r, c] = makeDes ? CellFlags.Destructible : CellFlags.Wall;
                    }
                    else
                    {
                        cells[r, c] = CellFlags.None;
                    }
                }
            }
        }
    }

    // ── 좌표 유틸: (r,c) <-> World ─────────────────────────────────
    public Vector3 CellToWorld(int r, int c)
    {
        // 항상 "셀 중앙" 좌표 반환
        return origin + new Vector3((c + 0.5f) * cellSize, (r + 0.5f) * cellSize, 0f);
    }

    public Vector2Int WorldToCell(Vector3 world)
    {
        Vector3 local = world - origin;
        int c = Mathf.FloorToInt(local.x / cellSize);
        int r = Mathf.FloorToInt(local.y / cellSize);

        r = Mathf.Clamp(r, 0, Height - 1);
        c = Mathf.Clamp(c, 0, Width - 1);
        return new Vector2Int(r, c); // (r,c)
    }

    // ── 범위/판정 ────────────────────────────────────────────────
    public bool InBounds(int r, int c) => r >= 0 && c >= 0 && r < Height && c < Width;

    public bool IsWalkable(int r, int c)
    {
        if (!InBounds(r, c)) return false;
        var f = cells[r, c];
        // "통과 불가" 조건만 제외: 벽 / 파괴가능벽 / 타워
        return (f & (CellFlags.Wall | CellFlags.Destructible | CellFlags.Tower)) == 0;
    }

    public bool IsWall(int r, int c) => InBounds(r, c) && (cells[r, c] & CellFlags.Wall) != 0;
    public bool IsDestructible(int r, int c) => InBounds(r, c) && (cells[r, c] & CellFlags.Destructible) != 0;
    public bool HasTower(int r, int c) => InBounds(r, c) && (cells[r, c] & CellFlags.Tower) != 0;

    public void SetWall(int r, int c, bool on)
    {
        if (!InBounds(r, c)) return;
        if (on) cells[r, c] |= CellFlags.Wall;
        else cells[r, c] &= ~CellFlags.Wall;
        OnCellChanged?.Invoke(r, c);
    }

    public void SetDestructible(int r, int c, bool on)
    {
        if (!InBounds(r, c)) return;
        if (on) cells[r, c] |= CellFlags.Destructible;
        else cells[r, c] &= ~CellFlags.Destructible;
        OnCellChanged?.Invoke(r, c);
    }

    public void SetTower(int r, int c, bool on)
    {
        if (!InBounds(r, c)) return;
        if (on) cells[r, c] |= CellFlags.Tower;
        else cells[r, c] &= ~CellFlags.Tower;
        OnCellChanged?.Invoke(r, c);
    }

    // ── 테스트: 클릭으로 타입 토글(옵션) ─────────────────────────
    public enum PaintMode { None, Wall, Destructible, Tower, Clear }

    [Header("Editor Paint (optional)")]
    public bool editorPaint = false;
    public PaintMode paintMode = PaintMode.None;

    void Update()
    {
        if (!editorPaint) return;

        if (Input.GetMouseButtonDown(0))
        {
            var wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            wp.z = 0f;
            var rc = WorldToCell(wp);

            switch (paintMode)
            {
                case PaintMode.Wall: SetWall(rc.x, rc.y, true); SetDestructible(rc.x, rc.y, false); SetTower(rc.x, rc.y, false); break;
                case PaintMode.Destructible: SetWall(rc.x, rc.y, false); SetDestructible(rc.x, rc.y, true); SetTower(rc.x, rc.y, false); break;
                case PaintMode.Tower: SetWall(rc.x, rc.y, false); SetDestructible(rc.x, rc.y, false); SetTower(rc.x, rc.y, true); break;
                case PaintMode.Clear: SetWall(rc.x, rc.y, false); SetDestructible(rc.x, rc.y, false); SetTower(rc.x, rc.y, false); break;
            }
        }
    }
}
