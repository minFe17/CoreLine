using System.Collections.Generic;
using UnityEngine;
using static TestMap;

public class GridVisualizer : MonoBehaviour
{
    [SerializeField] private TestMap _map;

    [Header("Overlay (optional)")]
    [SerializeField] private bool _useOverlay = false;
    [SerializeField] private GameObject _cellOverlayPrefab;        

    [Header("Colors")]
    [SerializeField] private Color _walkableColor = new Color(0.2f, 1f, 0.6f, 0.12f);
    [SerializeField] private Color _wallColor = new Color(1f, 0.2f, 0.2f, 0.45f);
    [SerializeField] private Color _destructibleColor = new Color(0.9f, 0.2f, 0.9f, 0.45f);
    [SerializeField] private Color _towerColor = new Color(1f, 0.6f, 0.2f, 0.5f);

    private GameObject[,] _overlays;

    private void Awake()
    {
        if (!_map) _map = FindAnyObjectByType<TestMap>();
    }

    private void OnEnable()
    {
        if (_map != null) _map.OnCellChanged += HandleCellChanged;
        RebuildAll();
    }

    private void OnDisable()
    {
        if (_map != null) _map.OnCellChanged -= HandleCellChanged;
        ClearAll();
    }

    private void HandleCellChanged(int r, int c)
    {
        if (!_useOverlay || _overlays == null) return;
        UpdateCell(r, c);
    }

    public void RebuildAll()
    {
        if (!_map) return;

        if (_useOverlay && _cellOverlayPrefab != null)
        {
            ClearAll();
            _overlays = new GameObject[_map.Height, _map.Width];

            for (int r = 0; r < _map.Height; r++)
            {
                for (int c = 0; c < _map.Width; c++)
                {
                    GameObject go = Instantiate(_cellOverlayPrefab, _map.CellToWorld(r, c), Quaternion.identity, transform);
                    go.transform.localScale = new Vector3(_map.CellSize, _map.CellSize, 1f);
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

    private void UpdateCell(int r, int c)
    {
        if (_overlays[r, c] == null) return;
        ApplyColor(r, c);
    }

    private void ApplyColor(int r, int c)
    {
        CellFlags f = _map.cells[r, c];
        SpriteRenderer sr = _overlays[r, c].GetComponent<SpriteRenderer>();
        if (!sr) return;

        if ((f & TestMap.CellFlags.Wall) != 0) sr.color = _wallColor;
        else if ((f & TestMap.CellFlags.Tower) != 0) sr.color = _towerColor;
        else if ((f & TestMap.CellFlags.Destructible) != 0) sr.color = _destructibleColor;
        else sr.color = _walkableColor;
    }

    private void OnDrawGizmos()
    {
        if (!_map || _useOverlay) return;

        for (int r = 0; r < _map.Height; r++)
        {
            for (int c = 0; c < _map.Width; c++)
            {
                CellFlags f = _map.cells != null ? _map.cells[r, c] : TestMap.CellFlags.None;
                Color col = _walkableColor;
                if ((f & TestMap.CellFlags.Wall) != 0) col = _wallColor;
                else if ((f & TestMap.CellFlags.Tower) != 0) col = _towerColor;
                else if ((f & TestMap.CellFlags.Destructible) != 0) col = _destructibleColor;

                Gizmos.color = col;
                Vector3 p = _map.CellToWorld(r, c);
                Gizmos.DrawCube(p, new Vector3(_map.CellSize * 0.98f, _map.CellSize * 0.98f, 0.1f));
            }
        }
    }
}
