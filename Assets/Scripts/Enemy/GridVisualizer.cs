using System.Collections.Generic;
using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    [Header("Refs")]
    public TestMap map;

    [Header("Overlay (optional)")]
    public bool useOverlay = false;                   // �ΰ��� ǥ��
    public GameObject cellOverlayPrefab;              // ������ Quad/Sprite ������(����)

    [Header("Colors")]
    public Color walkableColor = new Color(0.2f, 1f, 0.6f, 0.12f);
    public Color wallColor = new Color(1f, 0.2f, 0.2f, 0.45f);
    public Color destructibleColor = new Color(0.9f, 0.2f, 0.9f, 0.45f);
    public Color towerColor = new Color(1f, 0.6f, 0.2f, 0.5f);

    private GameObject[,] _overlays;

    void Awake()
    {
        if (!map) map = FindAnyObjectByType<TestMap>();
    }

    void OnEnable()
    {
        if (map != null) map.OnCellChanged += HandleCellChanged;
        RebuildAll();
    }

    void OnDisable()
    {
        if (map != null) map.OnCellChanged -= HandleCellChanged;
        ClearAll();
    }

    void HandleCellChanged(int r, int c)
    {
        if (!useOverlay || _overlays == null) return;
        UpdateCell(r, c);
    }

    public void RebuildAll()
    {
        if (!map) return;

        if (useOverlay && cellOverlayPrefab != null)
        {
            ClearAll();
            _overlays = new GameObject[map.Height, map.Width];

            for (int r = 0; r < map.Height; r++)
            {
                for (int c = 0; c < map.Width; c++)
                {
                    var go = Instantiate(cellOverlayPrefab, map.CellToWorld(r, c), Quaternion.identity, transform);
                    go.transform.localScale = new Vector3(map.CellSize, map.CellSize, 1f);
                    _overlays[r, c] = go;
                    ApplyColor(r, c);
                }
            }
        }
    }

    public void ClearAll()
    {
        if (_overlays == null) return;
        for (int r = 0; r < _overlays.GetLength(0); r++)
            for (int c = 0; c < _overlays.GetLength(1); c++)
                if (_overlays[r, c]) Destroy(_overlays[r, c]);
        _overlays = null;
    }

    void UpdateCell(int r, int c)
    {
        if (_overlays[r, c] == null) return;
        ApplyColor(r, c);
    }

    void ApplyColor(int r, int c)
    {
        var f = map.cells[r, c];
        var sr = _overlays[r, c].GetComponent<SpriteRenderer>();
        if (!sr) return;

        // �켱����: Wall > Tower > Destructible > Walkable
        if ((f & TestMap.CellFlags.Wall) != 0) sr.color = wallColor;
        else if ((f & TestMap.CellFlags.Tower) != 0) sr.color = towerColor;
        else if ((f & TestMap.CellFlags.Destructible) != 0) sr.color = destructibleColor;
        else sr.color = walkableColor;
    }

    // ������ �信���� �����ε� �����ֱ�(�ΰ��� off�� ��)
    void OnDrawGizmos()
    {
        if (!map || useOverlay) return;

        for (int r = 0; r < map.Height; r++)
        {
            for (int c = 0; c < map.Width; c++)
            {
                var f = map.cells != null ? map.cells[r, c] : TestMap.CellFlags.None;
                Color col = walkableColor;
                if ((f & TestMap.CellFlags.Wall) != 0) col = wallColor;
                else if ((f & TestMap.CellFlags.Tower) != 0) col = towerColor;
                else if ((f & TestMap.CellFlags.Destructible) != 0) col = destructibleColor;

                Gizmos.color = col;
                var p = map.CellToWorld(r, c);
                Gizmos.DrawCube(p, new Vector3(map.CellSize * 0.98f, map.CellSize * 0.98f, 0.1f));
            }
        }
    }
}
