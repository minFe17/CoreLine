using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

/// <summary>
/// ºôµå ¿É¼Ç 1°³¿¡ ´ëÇÑ µ¥ÀÌÅÍ(¾ÆÀÌÄÜ/ÇÁ¸®ÆÕ/ºñ¿ë µî).
/// ÆÄÀÏ ºÐ¸®ÇØµµ µÇÁö¸¸, µ¥¸ð ÆíÀÇ¸¦ À§ÇØ °°Àº ÆÄÀÏ¿¡ µÓ´Ï´Ù.
/// </summary>
public struct TowerOption
{
    public string Id;
    public Sprite Icon;
    public GameObject Prefab;
    public int Cost;

    public TowerOption(string id, Sprite icon, GameObject prefab, int cost)
    { Id = id; Icon = icon; Prefab = prefab; Cost = cost; }
}

/// <summary>
/// Å¸ÀÏÀ» Å¬¸¯ÇßÀ» ¶§, Å¬¸¯ÇÑ Å¸ÀÏÀÇ **Á¤Áß¾Ó**À» ±âÁØÀ¸·Î
/// ÁÂ/¿ì¿¡ °¢°¢ 2¡¿2 ±×¸®µå¸¦ ºÙ¿© º¸¿©ÁÖ´Â ºôµå UI ÄÁÆ®·Ñ·¯.
///
/// - BuildPanel ÀÌ¸§ÀÇ RectTransformÀ» '±âÁØ »ç°¢Çü'À¸·Î »ç¿ë(ÁÂÇ¥ º¯È¯ ¿ÀÂ÷ ¹æÁö)
/// - BuildPanelÀ» ÄÚµå¿¡¼­ ÀüÃ¼È­¸é stretch º¸Áõ(¾ÞÄ¿/¿ÀÇÁ¼Â °­Á¦)
/// - ÅÛÇÃ¸´(ButtonTemplate)Àº Àý´ë ÆÄ±«ÇÏÁö ¾Ê°í, ¼û±è º¹Á¦º»(ghost)À¸·Î¸¸ ÀÎ½ºÅÏ½ºÈ­
/// - È­¸é °¡ÀåÀÚ¸®¿¡¼­´Â ÇÑÂÊ ±×¸®µå¸¸ ³ëÃâ(ÀÚµ¿ º¸Á¤)
/// </summary>
public class BuildUI : MonoBehaviour
{
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ·¹ÀÌ¾Æ¿ô: ÁÂ/¿ì °¢°¢ 2¡¿2 = ÃÑ 8Ä­
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    [Header("Grid Layout (per side)")]
    [SerializeField] int cols = 2;                     // ¿­ ¼ö(°íÁ¤: 2)
    [SerializeField] int rows = 2;                     // Çà ¼ö(°íÁ¤: 2)
    [SerializeField] float cell = 96f;                 // ¹öÆ° ÇÑ º¯(px)
    [SerializeField] float spacing = 8f;               // ¹öÆ° °£°Ý(px)
    [SerializeField] float gapFromTile = 16f;          // Å¸ÀÏ Áß½É°ú ±×¸®µå »çÀÌ °Å¸®(px)
    [SerializeField] bool closeOnBackground = true;    // Dimmer Å¬¸¯ ½Ã ´Ý±â
    [SerializeField] float clusterGap = 12f; // °°Àº Æí¿¡ 2x2 ¹­À½ µÎ °³¸¦ ³ª¶õÈ÷ µÑ ¶§ »çÀÌ °£°Ý
    private Vector2 _lastGridSize;           // LayoutGrids ¶§ °è»êÇÑ 2x2 ÇÏ³ªÀÇ ½ÇÁ¦ ÇÈ¼¿ Å©±â
    // Äµ¹ö½º/ÇÊ¼ö ³ëµå
    private Canvas _canvas;
    private RectTransform _root;        // BuildPanel(±âÁØ »ç°¢Çü)
    private RectTransform _dimmer;      // ¹è°æ(Å¬¸¯ ½Ã ´Ý±â)
    private RectTransform _anchor;      // Å¸ÀÏ Áß¾ÓÀ» °¡¸®Å°´Â ºó RT
    private RectTransform _leftGrid;    // ÁÂÃø 2¡¿2 ±×¸®µå
    private RectTransform _rightGrid;   // ¿ìÃø 2¡¿2 ±×¸®µå

    private Vector3Int _currentCell;
    private bool _hasCurrentCell;
    private Action<TowerOption, Vector3Int> _onPickCell;
    // ¹öÆ° ÇÁ¸®ÆÕ (TowerPlaceButtonÀº BaseButton »ó¼Ó¹ÞÀº Àü¿ë ¹öÆ° Å¬·¡½º)
    [Header("Prefabs")]
    [SerializeField] private TowerPlaceButton buttonPrefab; // ÀÎ½ºÆåÅÍ¿¡¼­ µå·¡±×ÇØµÎ¸é ¹Ù·Î »ç¿ë °¡´É
    private TowerPlaceButton _runtimeButtonPrefab;          // ½ÇÁ¦ InstantiateÇÒ ¿øº»

    // ÇöÀç ¼¼¼ÇÀÇ ¼±ÅÃ ÄÝ¹é
    private Action<TowerOption> _onPick;

    private bool _wired; // ¹è¼± ¿Ï·á ¿©ºÎ

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Unity Lifecycle
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void Awake()
    {
        Wire();         // ÇÊ¿äÇÑ ÂüÁ¶/ÇÁ¸®ÆÕ ¹è¼±
        LogWireState(); // ¿¡µðÅÍ¿¡¼­ È®ÀÎ¿ë
        if (_root) _root.gameObject.SetActive(false); // ½ÃÀÛ ½Ã ´ÝÈû
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¿ÜºÎ API: Å¸ÀÏ(¿ùµå) À§Ä¡ ±âÁØÀ¸·Î ¿­±â
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    /// <summary>
    /// ¿ùµå ÁÂÇ¥ ±âÁØÀ¸·Î ¿­±â (ÄÝ¹é ¾øÀ½)
    /// </summary>
    public void OpenAtWorld(Vector3 worldPos, List<TowerOption> options)
        => OpenAtWorld(worldPos, options, null);

    /// <summary>
    /// ¼¿ ÁÂÇ¥ ±âÁØÀ¸·Î ¿­±â (ÄÝ¹é ¾øÀ½)
    /// </summary>
    public void OpenAtCell(Vector3Int cell, List<TowerOption> options)
        => OpenAtCell(cell, options, null);

    public void OpenAtCell(Vector3Int cell, List<EUnitType> options)
        => OpenAtCell(cell, options, null);

    /// <summary>
    /// ¿ùµå ÁÂÇ¥ ±âÁØÀ¸·Î ¿­±â (¼±ÅÃ ½Ã ½ÇÇàÇÒ ÄÝ¹é Àü´Þ °¡´É)
    /// </summary>
    public void OpenAtWorld(Vector3 worldPos, List<TowerOption> options, Action<TowerOption> onPick)
    {
        _onPick = onPick;

        _onPickCell = null;            // <- ¼¿ ¾ø´Â ÄÝ¹é ¸ðµå
        _hasCurrentCell = false;       // <- ¼¿ Á¤º¸ ÃÊ±âÈ­
        if (!_wired) { Wire(); LogWireState(); }
        if (!_wired || !_root || !_anchor || !_leftGrid || !_rightGrid) return;

        // 1) Å¸ÀÏ Áß¾ÓÀ¸·Î ½º³À
        MapManager map = MapManager.Instance;
        if (map && map.IsReady)
        {
            Vector3Int cell = map.WorldToCell(worldPos);
            worldPos = map.CellCenterWorld(cell);
            _currentCell = cell;       // <- ÇöÀç ¼¿ ÀúÀå
            _hasCurrentCell = true;
        }

        // 2) ¿ùµå¡æ½ºÅ©¸°¡æ·ÎÄÃ º¯È¯
        Camera uiCam = GetUiCamera();
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCam ?? Camera.main, worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screen, uiCam, out var local);
        _anchor.anchoredPosition = local;

        // 3) ±âÁ¸ ¹öÆ°µé Á¤¸®
        ClearChildrenExceptTemplate(_leftGrid, null);
        ClearChildrenExceptTemplate(_rightGrid, null);

        // 4) ¹öÆ° »ý¼º
        FillGrids(options);

        // 5) ·¹ÀÌ¾Æ¿ô/º¸Á¤
        LayoutGrids();
        AutoKeepInside();

        // 6) Ç¥½Ã
        _root.gameObject.SetActive(true);
    }
    public void OpenAtWorld(Vector3 worldPos, List<EUnitType> options, Action<TowerOption> onPick)
    {
        _onPick = onPick;

        _onPickCell = null;            // <- ¼¿ ¾ø´Â ÄÝ¹é ¸ðµå
        _hasCurrentCell = false;       // <- ¼¿ Á¤º¸ ÃÊ±âÈ­
        if (!_wired) { Wire(); LogWireState(); }
        if (!_wired || !_root || !_anchor || !_leftGrid || !_rightGrid) return;

        // 1) Å¸ÀÏ Áß¾ÓÀ¸·Î ½º³À
        MapManager map = MapManager.Instance;
        if (map && map.IsReady)
        {
            Vector3Int cell = map.WorldToCell(worldPos);
            worldPos = map.CellCenterWorld(cell);
            _currentCell = cell;       // <- ÇöÀç ¼¿ ÀúÀå
            _hasCurrentCell = true;
        }

        // 2) ¿ùµå¡æ½ºÅ©¸°¡æ·ÎÄÃ º¯È¯
        Camera uiCam = GetUiCamera();
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCam ?? Camera.main, worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screen, uiCam, out var local);
        _anchor.anchoredPosition = local;

        // 3) ±âÁ¸ ¹öÆ°µé Á¤¸®
        ClearChildrenExceptTemplate(_leftGrid, null);
        ClearChildrenExceptTemplate(_rightGrid, null);

        // 4) ¹öÆ° »ý¼º
        FillGrids(options);

        // 5) ·¹ÀÌ¾Æ¿ô/º¸Á¤
        LayoutGrids();
        AutoKeepInside();

        // 6) Ç¥½Ã
        _root.gameObject.SetActive(true);
    }


    /// <summary>
    /// ¼¿(±×¸®µå ÁÂÇ¥)À» ¹Ù·Î Àü´ÞÇÏ°í ½ÍÀº °æ¿ìÀÇ ÇïÆÛ.
    /// </summary>
    public void OpenAtCell(Vector3Int cell, List<TowerOption> options, Action<TowerOption, Vector3Int> onPickCell)
    {
        _onPick = null;          // ¼¿ ¾ø´Â ÄÝ¹éÀº ¾È ¾¸
        _onPickCell = null;      // ¡ç ÇÊµå¿¡ ÀúÀåÇÏÁö ¾Ê°í
        _currentCell = cell;
        _hasCurrentCell = true;

        MapManager map = MapManager.Instance;
        Vector3 world = map && map.IsReady ? map.CellCenterWorld(cell) : (Vector3)cell;

        // ¼¿(capture) Æ÷ÇÔ onPick ¶÷´Ù·Î À§ÀÓ
        OpenAtWorld(world, options, picked =>
        {
            onPickCell?.Invoke(picked, cell);
        });
    }

    public void OpenAtCell(Vector3Int cell, List<EUnitType> options, Action<TowerOption, Vector3Int> onPickCell)
    {
        _onPick = null;          // ¼¿ ¾ø´Â ÄÝ¹éÀº ¾È ¾¸
        _onPickCell = null;      // ¡ç ÇÊµå¿¡ ÀúÀåÇÏÁö ¾Ê°í
        _currentCell = cell;
        _hasCurrentCell = true;

        MapManager map = MapManager.Instance;
        Vector3 world = map && map.IsReady ? map.CellCenterWorld(cell) : (Vector3)cell;

        // ¼¿(capture) Æ÷ÇÔ onPick ¶÷´Ù·Î À§ÀÓ
        OpenAtWorld(world, options, picked =>
        {
            onPickCell?.Invoke(picked, cell);
        });
    }

    /// <summary>
    /// ´Ý±â(¾Ö´Ï¸ÞÀÌ¼Ç ÈÅÀ» ºÙÀÌ°í ½ÍÀ¸¸é ¿©±â¼­ Ã³¸®)
    /// </summary>
    public void Close()
    {
        _onPick = null;
        _onPickCell = null;
        _hasCurrentCell = false;
        if (_root) _root.gameObject.SetActive(false);
        SimpleSingleton<MediatorManager>.Instance.Notify(EMediatorType.EndSelectTile);
    }


    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ·¹ÀÌ¾Æ¿ô °è»ê (ÁÂ/¿ì 2¡¿2)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÆÄ±« °¡´É º® ´­·¶À»¶§ ·¹ÀÌ¾Æ¿ô

    void LayoutGrids()
    {
        // 2¡¿2 °íÁ¤
        void Apply(GridLayoutGroup gl)
        {
            gl.cellSize = new Vector2(cell, cell);
            gl.spacing = new Vector2(spacing, spacing);
            gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gl.constraintCount = cols; // °íÁ¤: 2
            gl.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gl.startAxis = GridLayoutGroup.Axis.Horizontal;
            gl.childAlignment = TextAnchor.UpperLeft;
        }
        Apply(_leftGrid.GetComponent<GridLayoutGroup>());
        Apply(_rightGrid.GetComponent<GridLayoutGroup>());

        // ±×¸®µå ¹Ú½º Å©±â Ä³½Ì(2¡¿2 ±âÁØ)
        Vector2 gridSize = new(
            cols * cell + (cols - 1) * spacing,
            rows * cell + (rows - 1) * spacing
        );
        _lastGridSize = gridSize;

        // ±âº» ¹èÄ¡: ¿ÞÂÊ/¿À¸¥ÂÊ
        _leftGrid.anchorMin = _leftGrid.anchorMax = new Vector2(0.5f, 0.5f);
        _leftGrid.pivot = new Vector2(1f, 0.5f);
        _leftGrid.sizeDelta = gridSize;
        _leftGrid.anchoredPosition = new Vector2(-gapFromTile, 0f);

        _rightGrid.anchorMin = _rightGrid.anchorMax = new Vector2(0.5f, 0.5f);
        _rightGrid.pivot = new Vector2(0f, 0.5f);
        _rightGrid.sizeDelta = gridSize;
        _rightGrid.anchoredPosition = new Vector2(+gapFromTile, 0f);
    }


    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // È­¸é °¡ÀåÀÚ¸® º¸Á¤(ÁÂ/¿ì °ø°£ÀÌ ³Ê¹« ¾øÀ¸¸é ÇÑÂÊ¸¸)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void AutoKeepInside()
    {
        Camera uiCam = GetUiCamera();
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCam, _anchor.position);
        float left = screen.x;
        float right = Screen.width - screen.x;
        const float edge = 80f; // ÀÌ °ªº¸´Ù ÀÛÀ¸¸é "±×ÂÊÀº ºñÁ¼´Ù"·Î ÆÇ´Ü

        // ±âº»Àº ¾çÂÊ º¸ÀÌµµ·Ï
        _leftGrid.gameObject.SetActive(true);
        _rightGrid.gameObject.SetActive(true);

        // ¿ÞÂÊÀÌ Á¼°í ¿À¸¥ÂÊÀº ³Ë³Ë ¡æ ¿À¸¥ÂÊÀ¸·Î 8Ä­ ¸ô¾ÆÁÖ±â
        if (left < edge && right >= edge)
        {
            // µÑ ´Ù "¿À¸¥ÂÊ ¸ðµå" ÇÇ¹þ/¾ÞÄ¿
            _rightGrid.anchorMin = _rightGrid.anchorMax = new Vector2(0.5f, 0.5f);
            _rightGrid.pivot = new Vector2(0f, 0.5f);
            _rightGrid.anchoredPosition = new Vector2(+gapFromTile, 0f); // ¾ÞÄ¿¿¡ ´õ °¡±î¿î ¹­À½

            _leftGrid.anchorMin = _leftGrid.anchorMax = new Vector2(0.5f, 0.5f);
            _leftGrid.pivot = new Vector2(0f, 0.5f);
            _leftGrid.anchoredPosition = new Vector2(+gapFromTile + _lastGridSize.x + clusterGap, 0f); // ±× ¿·¿¡ ºÙÀÌ±â

            return;
        }
        // ¿À¸¥ÂÊÀÌ Á¼°í ¿ÞÂÊÀº ³Ë³Ë ¡æ ¿ÞÂÊÀ¸·Î 8Ä­ ¸ô¾ÆÁÖ±â
        else if (right < edge && left >= edge)
        {
            // µÑ ´Ù "¿ÞÂÊ ¸ðµå" ÇÇ¹þ/¾ÞÄ¿
            _leftGrid.anchorMin = _leftGrid.anchorMax = new Vector2(0.5f, 0.5f);
            _leftGrid.pivot = new Vector2(1f, 0.5f);
            _leftGrid.anchoredPosition = new Vector2(-gapFromTile, 0f); // ¾ÞÄ¿¿¡ ´õ °¡±î¿î ¹­À½

            _rightGrid.anchorMin = _rightGrid.anchorMax = new Vector2(0.5f, 0.5f);
            _rightGrid.pivot = new Vector2(1f, 0.5f);
            _rightGrid.anchoredPosition = new Vector2(-gapFromTile - _lastGridSize.x - clusterGap, 0f); // ±× ¿·¿¡ ºÙÀÌ±â

            return;
        }

        // µÑ ´Ù ¾Ö¸ÅÇÏ¸é ¿ø·¡ ¾çÂÊ ¹èÄ¡ À¯Áö(ÀÌ¹Ì LayoutGrids¿¡¼­ ¼Â¾÷µÊ)
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¹è¼±/Å½»ö
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void Wire()
    {
        _wired = false;
        _canvas = GetComponentInParent<Canvas>(true);
        if (!_canvas) _canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);

        Debug.Log(transform.parent);
        if(transform.parent != _canvas.transform)
            transform.SetParent(_canvas.transform);

        // ÀÌ¸§¿¡ ÀÇÁ¸ÇÏÁö ¸»°í, ÀÚ±â ÀÚ½ÅÀ» ·çÆ®·Î
        _root = (RectTransform)transform;

        _dimmer = FindChildRT(_root, "Dimmer");
        _anchor = FindChildRT(_root, "Anchor");
        _leftGrid = FindChildRT(_root, "LeftGrid");
        _rightGrid = FindChildRT(_root, "RightGrid");

        StretchToFullScreen(_root);

        if (_dimmer && closeOnBackground)
        {
            Button bgBtn = _dimmer.GetComponent<Button>();
            if (bgBtn)
            {
                bgBtn.onClick.RemoveAllListeners();
                bgBtn.onClick.AddListener(Close);
            }
        }

        // ¹öÆ° ÇÁ¸®ÆÕ ÁØºñ
        _runtimeButtonPrefab = buttonPrefab;
        if (_runtimeButtonPrefab == null)
            _runtimeButtonPrefab = Resources.Load<TowerPlaceButton>("Map/TowerPlaceButton");

        _wired = (_canvas && _root && _anchor && _leftGrid && _rightGrid && _runtimeButtonPrefab);
    }

    void LogWireState()
    {
        Debug.Log(
            $"[BuildUI] wired={_wired}\n" +
            $" runtimePrefab={(_runtimeButtonPrefab ? _runtimeButtonPrefab.name : "NULL")}"
        );
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¹öÆ° Ã¤¿ì±â (ÃÖ´ë 8Ä­: ÁÂ4 + ¿ì4)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void FillGrids(List<TowerOption> options)
    {
        if (_runtimeButtonPrefab == null)
        { Debug.LogError("[BuildUI] No TowerPlaceButton prefab."); return; }

        options ??= new List<TowerOption>();

        int perSide = cols * rows;   // 2¡¿2 ¡æ 4
        int maxTotal = perSide * 2;  // 8
        int total = Mathf.Clamp(options.Count > 0 ? options.Count : maxTotal, 1, maxTotal);

        for (int i = 0; i < total; i++)
        {
            RectTransform parent = (i < perSide) ? _leftGrid : _rightGrid;
            TowerPlaceButton btn = Instantiate(_runtimeButtonPrefab, parent);
            btn.gameObject.SetActive(true);

            RectTransform rt = (RectTransform)btn.transform;
            if (rt.sizeDelta == Vector2.zero) rt.sizeDelta = new Vector2(cell, cell);

            if (i < options.Count)
            {
                TowerOption opt = options[i];
                btn.Bind(opt, picked =>
                {
                    if (_onPickCell != null && _hasCurrentCell)
                        _onPickCell.Invoke(picked, _currentCell);
                    else
                        _onPick?.Invoke(picked);

                    Close();
                });
            }
            else
            {
                btn.Bind((TowerOption)default, null); // ºó ½½·Ô
            }
        }
    }

    void FillGrids(List<EUnitType> options)
    {
        if (_runtimeButtonPrefab == null)
        { Debug.LogError("[BuildUI] No TowerPlaceButton prefab."); return; }

        options ??= new List<EUnitType>();

        int perSide = cols * rows;   // 2¡¿2 ¡æ 4
        int maxTotal = perSide * 2;  // 8
        int total = Mathf.Clamp(options.Count > 0 ? options.Count : maxTotal, 1, maxTotal);

        for (int i = 0; i < total; i++)
        {
            RectTransform parent = (i < perSide) ? _leftGrid : _rightGrid;
            TowerPlaceButton btn = Instantiate(_runtimeButtonPrefab, parent);
            btn.gameObject.SetActive(true);

            RectTransform rt = (RectTransform)btn.transform;
            if (rt.sizeDelta == Vector2.zero) rt.sizeDelta = new Vector2(cell, cell);

            if (i < options.Count)
            {
                EUnitType opt = options[i];
                btn.Bind(opt, picked =>
                {
                    if (_onPickCell != null && _hasCurrentCell)
                        _onPickCell.Invoke(picked, _currentCell);
                    else
                        _onPick?.Invoke(picked);

                    Close();
                });
            }
            else
            {
                btn.Bind((TowerOption)default, null); // ºó ½½·Ô
            }
        }
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ³»ºÎ À¯Æ¿
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    Camera GetUiCamera()
    {
        // Overlay ¡æ null, ±× ¿Ü(½ºÅ©¸°-Ä«¸Þ¶ó/¿ùµå) ¡æ canvas.worldCamera(¾øÀ¸¸é Main)
        if (_canvas == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
        return _canvas.worldCamera != null ? _canvas.worldCamera : Camera.main;
    }

    static void StretchToFullScreen(RectTransform rt)
    {
        if (!rt) return;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static RectTransform FindChildRT(Transform root, string name)
    {
        if (!root) return null;
        foreach (RectTransform rt in root.GetComponentsInChildren<RectTransform>(true))
            if (rt.name == name) return rt;
        return null;
    }

    static T FindChild<T>(Transform root, string name, bool includeInactive = false) where T : Component
    {
        if (!root) return null;
        foreach (var c in root.GetComponentsInChildren<T>(includeInactive))
            if (c.name == name) return c;
        return null;
    }

    static void ClearChildrenExceptTemplate(Transform t, Transform keep)
    {
        if (!t) return;
        for (int i = t.childCount - 1; i >= 0; --i)
        {
            var child = t.GetChild(i);
            if (keep && child == keep) continue; // ÅÛÇÃ¸´Àº º¸Á¸
            UnityEngine.Object.Destroy(child.gameObject);
        }
    }
}
//À¯´Ö¿¡¼­ »ç¿ë
//buildUI.OpenAtCell(cell, options, (picked, selectedCell) =>
//{
//    MapManager map = MapManager.Instance;
//    if (!map.GetPlaceInfo(selectedCell).Placeable) return;
//
//    Vector3 pos = map.CellCenterWorld(selectedCell); //Á¤Áß¾Ó
//});
