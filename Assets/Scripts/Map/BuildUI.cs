using System;
using System.Collections.Generic;
using UnityEditor;
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
    // ·¹ÀÌ¾Æ¿ô: ÁÂ/¿ì °¢°¢ 2¡¿2 = ÃÑ 8Ä­ (ÀÎ½ºÆåÅÍ ¹«½Ã, ÄÚµå °íÁ¤)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private const int COLS = 2;
    private const int ROWS = 2;

    private const float SPACING_PX = 20f;      // ¹öÆ° °£°Ý
    private const float GAP_FROM_TILE_PX = 20f;// Å¸ÀÏ Áß½É°ú ±×¸®µå »çÀÌ °Å¸®
    private const float CLUSTER_GAP_PX = 20f;  // °°Àº Æí ¹­À½ »çÀÌ °£°Ý

    private static readonly Vector2 FALLBACK_CELL = new(80f, 120f); // ÇÁ¸®ÆÕ Å©±â ¾øÀ¸¸é ±âº»

    private Vector2 _cellSizePx;               // ¹öÆ° Å©±â(ÇÁ¸®ÆÕ¡æ±âº» 80¡¿120)
    private Vector2 _lastGridSize;             // 2¡¿2 ÇÏ³ªÀÇ ½ÇÁ¦ ÇÈ¼¿ Å©±â
    [SerializeField] bool closeOnBackground = true;    // Dimmer Å¬¸¯ ½Ã ´Ý±â
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
    private void Awake()
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
        if (PauseControl.IsPaused) return;

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
        if (PauseControl.IsPaused) return;

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
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_root, screen, uiCam, out Vector2 local);
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
        if (PauseControl.IsPaused) return;

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
        if (PauseControl.IsPaused) return;

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

    private void LayoutGrids()
    {
        // 2¡¿2 °íÁ¤
        void Apply(GridLayoutGroup grid)
        {
            grid.cellSize = _cellSizePx;                       // ¡ç ÇÁ¸®ÆÕ(¶Ç´Â 80¡¿120)
            grid.spacing = new Vector2(SPACING_PX, SPACING_PX);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = COLS;
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperLeft;
        }
        Apply(_leftGrid.GetComponent<GridLayoutGroup>());
        Apply(_rightGrid.GetComponent<GridLayoutGroup>());

        // 2¡¿2 ±×¸®µå ¹Ú½º Å©±â
        Vector2 gridSize = new(
            COLS * _cellSizePx.x + (COLS - 1) * SPACING_PX,
            ROWS * _cellSizePx.y + (ROWS - 1) * SPACING_PX
        );
        _lastGridSize = gridSize;

        // ±âº» ¹èÄ¡: ¿ÞÂÊ/¿À¸¥ÂÊ
        _leftGrid.anchorMin = _leftGrid.anchorMax = new Vector2(0.5f, 0.5f);
        _leftGrid.pivot = new Vector2(1f, 0.5f);
        _leftGrid.sizeDelta = gridSize;
        _leftGrid.anchoredPosition = new Vector2(-GAP_FROM_TILE_PX, 0f);

        _rightGrid.anchorMin = _rightGrid.anchorMax = new Vector2(0.5f, 0.5f);
        _rightGrid.pivot = new Vector2(0f, 0.5f);
        _rightGrid.sizeDelta = gridSize;
        _rightGrid.anchoredPosition = new Vector2(+GAP_FROM_TILE_PX, 0f);
    }



    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // È­¸é °¡ÀåÀÚ¸® º¸Á¤(ÁÂ/¿ì °ø°£ÀÌ ³Ê¹« ¾øÀ¸¸é ÇÑÂÊ¸¸)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void AutoKeepInside()
{
    Camera uiCam = GetUiCamera();
    Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCam, _anchor.position);
    float left = screen.x;
    float right = Screen.width - screen.x;
    const float edge = 80f;

    _leftGrid.gameObject.SetActive(true);
    _rightGrid.gameObject.SetActive(true);

    if (left < edge && right >= edge)
    {
        _rightGrid.anchorMin = _rightGrid.anchorMax = new Vector2(0.5f, 0.5f);
        _rightGrid.pivot = new Vector2(0f, 0.5f);
        _rightGrid.anchoredPosition = new Vector2(+GAP_FROM_TILE_PX, 0f);

        _leftGrid.anchorMin = _leftGrid.anchorMax = new Vector2(0.5f, 0.5f);
        _leftGrid.pivot = new Vector2(0f, 0.5f);
        _leftGrid.anchoredPosition = new Vector2(+GAP_FROM_TILE_PX + _lastGridSize.x + CLUSTER_GAP_PX, 0f);
        return;
    }
    else if (right < edge && left >= edge)
    {
        _leftGrid.anchorMin = _leftGrid.anchorMax = new Vector2(0.5f, 0.5f);
        _leftGrid.pivot = new Vector2(1f, 0.5f);
        _leftGrid.anchoredPosition = new Vector2(-GAP_FROM_TILE_PX, 0f);

        _rightGrid.anchorMin = _rightGrid.anchorMax = new Vector2(0.5f, 0.5f);
        _rightGrid.pivot = new Vector2(1f, 0.5f);
        _rightGrid.anchoredPosition = new Vector2(-GAP_FROM_TILE_PX - _lastGridSize.x - CLUSTER_GAP_PX, 0f);
        return;
    }
}


    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¹è¼±/Å½»ö
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void Wire()
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
            Button backButton = _dimmer.GetComponent<Button>();
            if (backButton)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(Close);
            }
        }

        // ¹öÆ° ÇÁ¸®ÆÕ ÁØºñ
        _runtimeButtonPrefab = buttonPrefab;
        if (_runtimeButtonPrefab == null)
            _runtimeButtonPrefab = Resources.Load<TowerPlaceButton>("Map/TowerPlaceButton");

        _wired = (_canvas && _root && _anchor && _leftGrid && _rightGrid && _runtimeButtonPrefab);

        // ÇÁ¸®ÆÕ RectTransform »çÀÌÁî ¡æ ¹öÆ° Å©±â °áÁ¤(¾øÀ¸¸é ±âº» 80¡¿120)
        _cellSizePx = GetPrefabButtonSize(_runtimeButtonPrefab);
    }
    private Vector2 GetPrefabButtonSize(TowerPlaceButton prefab)
    {
        if (!prefab) return FALLBACK_CELL;
        var rt = prefab.GetComponent<RectTransform>();
        if (rt && rt.sizeDelta != Vector2.zero) return rt.sizeDelta;
        return FALLBACK_CELL;
    }
    private void LogWireState()
    {
        Debug.Log(
            $"[BuildUI] wired={_wired}\n" +
            $" runtimePrefab={(_runtimeButtonPrefab ? _runtimeButtonPrefab.name : "NULL")}"
        );
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¹öÆ° Ã¤¿ì±â (ÃÖ´ë 8Ä­: ÁÂ4 + ¿ì4)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void FillGrids(List<TowerOption> options)
    {
        if (_runtimeButtonPrefab == null)
        { Debug.LogError("[BuildUI] No TowerPlaceButton prefab."); return; }

        options ??= new List<TowerOption>();

        // ¡å ¹öÆ° Å©±â: ÇÁ¸®ÆÕ RT ±âÁØ, ¾øÀ¸¸é 80x120 ±âº»
        Vector2 cellSize = GetPrefabButtonSize(_runtimeButtonPrefab, new Vector2(80f, 120f));

        // ¡å 2¡¿2 °íÁ¤
        const int perSide = 4;   // 2x2
        const int maxTotal = 8;  // ÁÂ4 + ¿ì4
        int total = Mathf.Clamp(options.Count > 0 ? options.Count : maxTotal, 1, maxTotal);

        for (int i = 0; i < total; i++)
        {
            RectTransform parent = (i < perSide) ? _leftGrid : _rightGrid;
            TowerPlaceButton button = Instantiate(_runtimeButtonPrefab, parent);
            button.gameObject.SetActive(true);

            // ¡å ÇÁ¸®ÆÕ(¶Ç´Â ±âº») Å©±â·Î °­Á¦
            RectTransform rect = (RectTransform)button.transform;
            rect.sizeDelta = cellSize;

            if (i < options.Count)
            {
                TowerOption opt = options[i];
                button.Bind(opt, picked =>
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
                button.Bind((TowerOption)default, null); // ºó ½½·Ô
            }
        }

    }

    private void FillGrids(List<EUnitType> options)
    {
        if (_runtimeButtonPrefab == null)
        { Debug.LogError("[BuildUI] No TowerPlaceButton prefab."); return; }

        options ??= new List<EUnitType>();

        const int perSide = 4;   // 2x2
        const int maxTotal = 8;  // ÁÂ4 + ¿ì4
        int total = Mathf.Clamp(options.Count > 0 ? options.Count : maxTotal, 1, maxTotal);

        Vector2 cellSize = GetPrefabButtonSize(_runtimeButtonPrefab, new Vector2(80f, 120f));
        for (int i = 0; i < total; i++)
        {
            RectTransform parent = (i < perSide) ? _leftGrid : _rightGrid;
            TowerPlaceButton button = Instantiate(_runtimeButtonPrefab, parent);
            button.gameObject.SetActive(true);

            // ¡å ÇÁ¸®ÆÕ(¶Ç´Â ±âº») Å©±â·Î °­Á¦
            RectTransform rect = (RectTransform)button.transform;
            rect.sizeDelta = cellSize;

            if (i < options.Count)
            {
                EUnitType opt = options[i];
                button.Bind(opt, picked =>
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
                button.Bind((EUnitType)default, null); // ºó ½½·Ô
            }
        }
    }

    // --- ·ÎÄÃ ÇïÆÛ ---
    static Vector2 GetPrefabButtonSize(TowerPlaceButton prefab, Vector2 fallback)
    {
        if (!prefab) return fallback;
        var rt = prefab.GetComponent<RectTransform>();
        if (rt && rt.sizeDelta != Vector2.zero) return rt.sizeDelta;
        return fallback;
    }
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ³»ºÎ À¯Æ¿
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

    private Camera GetUiCamera()
    {
        // Overlay ¡æ null, ±× ¿Ü(½ºÅ©¸°-Ä«¸Þ¶ó/¿ùµå) ¡æ canvas.worldCamera(¾øÀ¸¸é Main)
        if (_canvas == null || _canvas.renderMode == RenderMode.ScreenSpaceOverlay) return null;
        return _canvas.worldCamera != null ? _canvas.worldCamera : Camera.main;
    }

    static void StretchToFullScreen(RectTransform rect)
    {
        if (!rect) return;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    static RectTransform FindChildRT(Transform root, string name)
    {
        if (!root) return null;
        foreach (RectTransform rect in root.GetComponentsInChildren<RectTransform>(true))
            if (rect.name == name) return rect;
        return null;
    }

    static T FindChild<T>(Transform root, string name, bool includeInactive = false) where T : Component
    {
        if (!root) return null;
        foreach (var c in root.GetComponentsInChildren<T>(includeInactive))
            if (c.name == name) return c;
        return null;
    }

    static void ClearChildrenExceptTemplate(Transform transform, Transform keep)
    {
        if (!transform) return;
        for (int i = transform.childCount - 1; i >= 0; --i)
        {
            var child = transform.GetChild(i);
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
