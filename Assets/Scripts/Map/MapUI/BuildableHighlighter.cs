using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // UI À§ Å¬¸¯ ¹«½Ã¿ë

/// <summary>
/// - ±âº»: ¿Ü°û¼± ºñÇ¥½Ã
/// - ¾Æ¹« Å¸ÀÏÀÌµç ÅÍÄ¡/Å¬¸¯ ½Ã: ºôµå °¡´É ÀüÃ¼ ¿Ü°û¼± Ç¥½Ã
/// - ±×¶§ ÅÇÇÑ Å¸ÀÏÀÌ ºôµå °¡´ÉÀÌ¸é ÇØ´ç Å¸ÀÏ¸¸ ¹ÝÂ¦
/// - (½Å±Ô) ½ºÅ×ÀÌÁö°¡ ·Îµå ¿Ï·á(GameManager.IsStageLoaded == true)µÈ µÚºÎÅÍ¸¸ µ¿ÀÛ
/// - (¿É¼Ç) º£ÀÌ½º ¹Ì¹èÄ¡ ½Ã Å·Å¸ÀÏ ÆÞ½º °­Á¶
/// - (¿É¼Ç) autoHideSec > 0 ÀÌ¸é ºôµå°¡´É °­Á¶ ÀÚµ¿ ¼û±è
/// </summary>
[DefaultExecutionOrder(1000)]
public class BuildableHighlighter : MonoBehaviour
{
    [Header("ÀÏ¹Ý ¿Ü°û¼±(ÀüÃ¼ Ç¥½Ã½Ã)")]
    [SerializeField] private Color _baseColor = new Color(0f, 1f, 0.6f, 0.55f);
    private float _baseWidth = 0.05f;

    [Header("°­Á¶(¼±ÅÃ ¼¿)")]
    [SerializeField] private Color _hoverColor = new Color(0.2f, 1f, 0.9f, 1f);
    [SerializeField] private float _hoverWidth = 0.05f;
    [SerializeField] private bool _hoverPulse = true;
    [SerializeField] private float _pulseSpeed = 2.2f;      // ±ôºýÀÓ ¼Óµµ
    [SerializeField] private float _pulseWidthAmp = 0.015f; // µÎ²² ÆßÇÎ
    [SerializeField] private float _pulseAlphaAmp = 0.25f;  // ¾ËÆÄ ÆßÇÎ(0~1 °¡ÁßÄ¡)

    [Header("Å·Å¸ÀÏ °­Á¶(º£ÀÌ½º ¹Ì¹èÄ¡ ½Ã Ç×»ó Ç¥½Ã)")]
    [SerializeField] private bool _highlightKingsUntilBasePlaced = true;
    [SerializeField] private Color _kingColor = new Color(1f, 0.85f, 0.2f, 0.95f);
    [SerializeField] private float _kingWidth = 0.055f;
    [SerializeField] private bool _kingPulse = true;            // ÆÞ½º ¿©ºÎ
    [SerializeField] private float _kingPulseSpeed = 2.0f;      // ÆÞ½º ¼Óµµ
    [SerializeField] private float _kingPulseWidthAmp = 0.02f;  // µÎ²² ÆßÇÎ
    [SerializeField] private float _kingPulseAlphaAmp = 0.2f;   // ¾ËÆÄ ÆßÇÎ

    [Header("Ç¥½Ã/¼û±è ¿É¼Ç (ºôµå°¡´É ÇÏÀÌ¶óÀÌÆ®¿¡¸¸ Àû¿ë)")]
    [SerializeField] private float _autoHideSec = 1f; // -1ÀÌ¸é ÀÚµ¿ ¼û±è ¾ÈÇÔ

    [Header("Á¤·Ä(2D ±âÁØ)")]
    [SerializeField] private string _sortingLayerName = "Default";
    [SerializeField] private int _sortingOrder = 1000;

    private Material _baseMat;
    private Material _hoverMat;
    private Material _kingMat;

    private readonly List<LineRenderer> _poolBuildables = new();
    private int _usedBuildables; // ÀÌ¹ø ÇÁ·¹ÀÓ ºôµå°¡´É ¶óÀÎ »ç¿ë ¼ö

    private readonly List<LineRenderer> _poolKings = new();
    private int _usedKings; // ÀÌ¹ø ÇÁ·¹ÀÓ Å· ¶óÀÎ »ç¿ë ¼ö

    private LineRenderer _hoverLR;

    private MapManager _map;
    private Camera _cam;
    private Vector3 _cellSize = Vector3.one;

    // »óÅÂ
    private bool _stageReady = false;      // ¡Ú ½ºÅ×ÀÌÁö ·Îµå(Global Flag) ¿Ï·á ¿©ºÎ
    private bool _showAll = false;
    private Vector3Int _selectedCell;
    private bool _selectedPlaceable = false;
    private float _hideAt = -1f;
    [SerializeField] bool buildablesOnlyAfterBasePlaced = true; // º£ÀÌ½º Àü: ºñÈ°¼º, ÈÄ: È°¼º

    public void SetCamera(Camera cam) => _cam = cam;

    private void Awake()
    {
        _cam = Camera.main ?? _cam;

        var shader = Shader.Find("Sprites/Default");
        _baseMat = new Material(shader); _baseMat.color = _baseColor;
        _hoverMat = new Material(shader); _hoverMat.color = _hoverColor;
        _kingMat = new Material(shader); _kingMat.color = _kingColor;
    }

    private void OnEnable()
    {
        // ½ºÅ×ÀÌÁö ·Îµå ¿Ï·á ÀÌº¥Æ® ±¸µ¶
        EventManager.Instance.Subscribe<string>(GameManager.EVT_STAGE_LOADED, OnStageLoaded);

        // ÀÌ¹Ì ·ÎµåµÈ »óÅÂ¶ó¸é Áï½Ã ÁØºñ
        if (GameManager.IsStageLoaded)
            OnStageLoaded(GameManager.LastLoadedStageId);
    }

    private void OnDisable()
    {
        // ÀÌº¥Æ® ÇØÁ¦
        EventManager.Instance.UnSubscribe(GameManager.EVT_STAGE_LOADED, (System.Action<string>)OnStageLoaded);
        UnhookMapEvents();
    }

    private void Update()
    {
        if (!_stageReady) return;          // ¡Ú ½ºÅ×ÀÌÁö ÁØºñ Àü¿¡´Â ¾Æ¹« °Íµµ ¾È ÇÔ
        if (PauseControl.IsPaused) return;

        if (_map == null || !_map.IsReady) return;

        // ¦¡¦¡ º£ÀÌ½º ¹Ì¹èÄ¡ ±¸°£: Å·Å¸ÀÏ¸¸ »ó½Ã °­Á¶, ºôµå°¡´É ÇÏÀÌ¶óÀÌÆ®´Â ¾Æ¿¹ ²û ¦¡¦¡
        if (buildablesOnlyAfterBasePlaced && !_map.HasPlayerBase)
        {
            _showAll = false;
            _selectedPlaceable = false;
            _hideAt = -1f;

            _usedBuildables = 0;
            _usedKings = 0;

            if (_highlightKingsUntilBasePlaced)
                DrawAllKings();

            DisableUnusedPool(_poolBuildables, 0, true);
            DisableUnusedPool(_poolKings, _usedKings);
            if (_hoverLR) _hoverLR.enabled = false;
            return;
        }

        // ¦¡¦¡ º£ÀÌ½º ¹èÄ¡ ÀÌÈÄ: ±âÁ¸ ·ÎÁ÷(ÅÇ ¡æ ºôµå°¡´É Ç¥½Ã + autoHide) ¦¡¦¡
        if (JustTapped(out Vector3 world) && !IsPointerOverUI())
        {
            world.z = 0f;
            _selectedCell = _map.WorldToCell(world);
            _selectedPlaceable = _map.GetPlaceInfo(_selectedCell).Placeable;

            _showAll = true;
            if (_autoHideSec > 0f) _hideAt = Time.unscaledTime + _autoHideSec;
        }

        if (_showAll && _autoHideSec > 0f && Time.unscaledTime >= _hideAt)
        {
            _showAll = false;
            _selectedPlaceable = false;
            _hideAt = -1f;
        }

        _usedBuildables = 0;
        _usedKings = 0;

        // º£ÀÌ½º°¡ ÀÌ¹Ì ÀÖÀ¸¹Ç·Î Å· °­Á¶´Â ÇÊ¿ä ¾øÀ½(È¤½Ã ³²¾ÆÀÖ´Ù¸é ´Ý±â)
        DisableUnusedPool(_poolKings, 0, true);

        if (_showAll)
        {
            DrawAllBuildables();
            UpdateHoverSelected();
        }
        else
        {
            DisableUnusedPool(_poolBuildables, _usedBuildables);
            if (_hoverLR) _hoverLR.enabled = false;
        }

        DisableUnusedPool(_poolKings, _usedKings);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ½ºÅ×ÀÌÁö ·Îµå ¿Ï·á Ã³¸® (EVT_STAGE_LOADED ÇÚµé·¯)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void OnStageLoaded(string _stageId)
    {
        _map = MapManager.Instance;
        _stageReady = (_map != null && _map.IsReady);

        UnhookMapEvents();
        if (_stageReady)
        {
            _map.GetNavFrame(out _, out _, out _cellSize);
            HookMapEvents();
            ClearAllImmediate(); // ½ÃÀÛ ½Ã Ç¥½Ã ²¨µÎ±â
        }
    }

    private void HookMapEvents()
    {
        if (_map == null) return;
        _map.OnCellChanged += OnCellChanged;
        _map.OnPlayerBasePlaced += OnPlayerBasePlaced;
    }

    private void UnhookMapEvents()
    {
        if (_map == null) return;
        _map.OnCellChanged -= OnCellChanged;
        _map.OnPlayerBasePlaced -= OnPlayerBasePlaced;
    }

    // ¸Ê ¼¿ »óÅÂ º¯°æ ½Ã
    private void OnCellChanged(Vector3Int _) { /* ÇÊ¿ä ½Ã ÇÃ·¡±× ¼¼¿ö ´ÙÀ½ ÇÁ·¹ÀÓ °»½Å */ }

    // ÇÃ·¹ÀÌ¾î º£ÀÌ½º ¹èÄ¡ ¿Ï·á ½Ã: Å· °­Á¶ Á¾·á
    private void OnPlayerBasePlaced(Vector3Int _)
    {
        DisableUnusedPool(_poolKings, 0, forceAll: true);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÀüÃ¼ ºôµå°¡´É ¿Ü°û¼±
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void DrawAllBuildables()
    {
        BoundsInt bounds = _map.GetNavBounds();
        foreach (Vector3Int cell in bounds.allPositionsWithin)
        {
            var info = _map.GetPlaceInfo(cell);
            if (!info.Placeable) continue;

            Vector3 center = _map.CellCenterWorld(cell);
            LineRenderer lr = GetLR(_poolBuildables, ref _usedBuildables, "BuildableOutline_");
            SetupLR(lr, _baseMat, _baseWidth, _sortingOrder);
            SetSquare(lr, center, _cellSize);
            lr.enabled = true;
        }

        DisableUnusedPool(_poolBuildables, _usedBuildables);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Å·Å¸ÀÏ °­Á¶(»ó½Ã)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void DrawAllKings()
    {
        var kings = _map.GetAllKingCells();
        for (int i = 0; i < kings.Count; i++)
        {
            Vector3 center = _map.CellCenterWorld(kings[i]);
            LineRenderer lr = GetLR(_poolKings, ref _usedKings, "KingOutline_");

            float width = _kingWidth;
            float alphaMul = 1f;
            if (_kingPulse)
            {
                float s = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * _kingPulseSpeed);
                width += (s - 0.5f) * 2f * _kingPulseWidthAmp;
                alphaMul = Mathf.Clamp01(1f - _kingPulseAlphaAmp + s * _kingPulseAlphaAmp);
            }
            Color c = _kingColor; c.a *= alphaMul;
            _kingMat.color = c;

            SetupLR(lr, _kingMat, width, _sortingOrder + 2);
            SetSquare(lr, center, _cellSize);
            lr.enabled = true;
        }
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¼±ÅÃ ¼¿ °­Á¶(ÆÞ½º)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void UpdateHoverSelected()
    {
        if (_hoverLR == null)
        {
            _hoverLR = CreateLR("HoverOutline");
            _hoverLR.material = _hoverMat;
            _hoverLR.sortingLayerName = _sortingLayerName;
            _hoverLR.sortingOrder = _sortingOrder + 1;
            _hoverLR.enabled = false;
        }

        if (!_selectedPlaceable)
        {
            _hoverLR.enabled = false;
            return;
        }

        float width = _hoverWidth;
        float alphaMul = 1f;
        if (_hoverPulse)
        {
            float s = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * _pulseSpeed);
            width += (s - 0.5f) * 2f * _pulseWidthAmp;
            alphaMul = Mathf.Clamp01(1f - _pulseAlphaAmp + s * _pulseAlphaAmp);
        }

        Color c = _hoverColor; c.a *= alphaMul;
        _hoverLR.material.color = c;
        _hoverLR.widthMultiplier = width;

        Vector3 center = _map.CellCenterWorld(_selectedCell);
        SetSquare(_hoverLR, center, _cellSize);
        _hoverLR.enabled = true;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // LineRenderer Ç®/»ý¼º/¼Â¾÷
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private LineRenderer GetLR(List<LineRenderer> pool, ref int used, string namePrefix)
    {
        if (used < pool.Count)
            return pool[used++];

        LineRenderer lr = CreateLR($"{namePrefix}{pool.Count}");
        pool.Add(lr);
        used++;
        return lr;
    }

    private LineRenderer CreateLR(string name)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(transform, false);
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = false;
        lr.textureMode = LineTextureMode.Stretch;
        lr.numCornerVertices = 2;
        lr.numCapVertices = 0;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.sortingLayerName = _sortingLayerName;
        lr.sortingOrder = _sortingOrder;
        lr.enabled = false;
        return lr;
    }

    private void SetupLR(LineRenderer lr, Material mat, float width, int sortOrder)
    {
        lr.material = mat;
        lr.widthMultiplier = width;
        lr.sortingLayerName = _sortingLayerName;
        lr.sortingOrder = sortOrder;
    }

    static readonly Vector3[] _pts = new Vector3[5];
    private void SetSquare(LineRenderer lr, Vector3 center, Vector3 size)
    {
        float hx = size.x * 0.5f;
        float hy = size.y * 0.5f;

        _pts[0] = new Vector3(center.x - hx, center.y - hy, 0f);
        _pts[1] = new Vector3(center.x - hx, center.y + hy, 0f);
        _pts[2] = new Vector3(center.x + hx, center.y + hy, 0f);
        _pts[3] = new Vector3(center.x + hx, center.y - hy, 0f);
        _pts[4] = _pts[0];

        lr.positionCount = 5;
        lr.SetPositions(_pts);
    }

    private void DisableUnusedPool(List<LineRenderer> pool, int usedCount, bool forceAll = false)
    {
        int start = forceAll ? 0 : usedCount;
        for (int i = start; i < pool.Count; i++)
            if (pool[i] && pool[i].enabled) pool[i].enabled = false;
    }

    private void ClearAllImmediate()
    {
        _showAll = false;
        _selectedPlaceable = false;
        _hideAt = -1f;

        DisableUnusedPool(_poolBuildables, 0, forceAll: true);
        DisableUnusedPool(_poolKings, 0, forceAll: true);

        if (_hoverLR) _hoverLR.enabled = false;
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÀÔ·Â À¯Æ¿ (¸¶¿ì½º/ÅÍÄ¡ °øÅë 'ÅÇ' Ã¼Å©)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private bool JustTapped(out Vector3 world)
    {
        world = default;

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            world = _cam ? _cam.ScreenToWorldPoint(Input.mousePosition) : (Vector3)Input.mousePosition;
            return true;
        }
#endif
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                world = _cam ? _cam.ScreenToWorldPoint(touch.position) : (Vector3)touch.position;
                return true;
            }
        }
        return false;
    }

    private bool IsPointerOverUI()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return true;
#endif
        if (Input.touchCount > 0 && EventSystem.current != null)
        {
            Touch touch = Input.GetTouch(0);
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return true;
        }
        return false;
    }
}
