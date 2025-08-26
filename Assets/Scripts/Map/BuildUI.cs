using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    // Äµ¹ö½º/ÇÊ¼ö ³ëµå
    private Canvas _canvas;
    private RectTransform _root;        // BuildPanel(±âÁØ »ç°¢Çü)
    private RectTransform _dimmer;      // ¹è°æ(Å¬¸¯ ½Ã ´Ý±â)
    private RectTransform _anchor;      // Å¸ÀÏ Áß¾ÓÀ» °¡¸®Å°´Â ºó RT
    private RectTransform _leftGrid;    // ÁÂÃø 2¡¿2 ±×¸®µå
    private RectTransform _rightGrid;   // ¿ìÃø 2¡¿2 ±×¸®µå

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

    /// <summary>
    /// ¿ùµå ÁÂÇ¥ ±âÁØÀ¸·Î ¿­±â (¼±ÅÃ ½Ã ½ÇÇàÇÒ ÄÝ¹é Àü´Þ °¡´É)
    /// </summary>
    public void OpenAtWorld(Vector3 worldPos, List<TowerOption> options, Action<TowerOption> onPick)
    {
        _onPick = onPick;

        if (!_wired) { Wire(); LogWireState(); }
        if (!_wired || !_root || !_anchor || !_leftGrid || !_rightGrid) return;

        // 1) Å¸ÀÏ Áß¾ÓÀ¸·Î ½º³À
        var map = MapManager.Instance;
        if (map && map.IsReady)
            worldPos = map.CellCenterWorld(map.WorldToCell(worldPos));

        // 2) ¿ùµå¡æ½ºÅ©¸°¡æ·ÎÄÃ º¯È¯
        var uiCam = GetUiCamera();
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
    public void OpenAtCell(Vector3Int cell, List<TowerOption> options, Action<TowerOption> onPick)
    {
        var map = MapManager.Instance;
        Vector3 world = map && map.IsReady ? map.CellCenterWorld(cell) : (Vector3)cell;
        OpenAtWorld(world, options, onPick);
    }

    /// <summary>
    /// ´Ý±â(¾Ö´Ï¸ÞÀÌ¼Ç ÈÅÀ» ºÙÀÌ°í ½ÍÀ¸¸é ¿©±â¼­ Ã³¸®)
    /// </summary>
    public void Close()
    {
        _onPick = null; // ¼¼¼Ç Á¾·á
        if (_root) _root.gameObject.SetActive(false);
    }


    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ·¹ÀÌ¾Æ¿ô °è»ê (ÁÂ/¿ì 2¡¿2)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void LayoutGrids()
    {
        // ±×¸®µå °øÅë ¼³Á¤(2¡¿2)
        void Apply(GridLayoutGroup gl)
        {
            gl.cellSize = new Vector2(cell, cell);
            gl.spacing = new Vector2(spacing, spacing);
            gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gl.constraintCount = cols; // 2¿­ °íÁ¤
            gl.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gl.startAxis = GridLayoutGroup.Axis.Horizontal;
            gl.childAlignment = TextAnchor.UpperLeft;
        }
        Apply(_leftGrid.GetComponent<GridLayoutGroup>());
        Apply(_rightGrid.GetComponent<GridLayoutGroup>());

        // ÇÑÂÊ ±×¸®µåÀÇ ½ÇÁ¦ ÇÈ¼¿ Å©±â
        Vector2 gridSize = new(
            cols * cell + (cols - 1) * spacing,   // 2*cell + spacing
            rows * cell + (rows - 1) * spacing    // 2*cell + spacing
        );

        // ¿ÞÂÊ ±×¸®µå: pivot(1,0.5) ¡æ ¿À¸¥ÂÊ ¸ð¼­¸®°¡ ¾ÞÄ¿(Å¸ÀÏ Áß¾Ó)¿¡ ´êÀ½
        _leftGrid.anchorMin = _leftGrid.anchorMax = new Vector2(0.5f, 0.5f);
        _leftGrid.pivot = new Vector2(1f, 0.5f);
        _leftGrid.sizeDelta = gridSize;
        _leftGrid.anchoredPosition = new Vector2(-gapFromTile, 0f);

        // ¿À¸¥ÂÊ ±×¸®µå: pivot(0,0.5) ¡æ ¿ÞÂÊ ¸ð¼­¸®°¡ ¾ÞÄ¿¿¡ ´êÀ½
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
        var uiCam = GetUiCamera();
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCam, _anchor.position);
        float left = screen.x;
        float right = Screen.width - screen.x;
        const float edge = 80f; // ±âÁØ ÇÈ¼¿(¿øÇÏ¸é ÀÎ½ºÆåÅÍ·Î »¬ °Í)

        _leftGrid.gameObject.SetActive(true);
        _rightGrid.gameObject.SetActive(true);

        if (left < edge && right >= edge) _leftGrid.gameObject.SetActive(false);
        else if (right < edge && left >= edge) _rightGrid.gameObject.SetActive(false);
        // µÑ ´Ù Á¼À¸¸é ±âº»(¾çÂÊ) À¯Áö ¡æ ÇÊ¿ä ½Ã ´õ °ø°ÝÀû º¸Á¤ ·ÎÁ÷ Ãß°¡
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ¹è¼±/Å½»ö
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    void Wire()
    {
        _wired = false;
        _canvas = GetComponentInParent<Canvas>(true);
        if (!_canvas) _canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);

        var panelRoot = FindChildRT(transform, "BuildPanel");
        if (!panelRoot) panelRoot = (RectTransform)transform;

        _root = panelRoot;
        _dimmer = FindChildRT(panelRoot, "Dimmer");
        _anchor = FindChildRT(panelRoot, "Anchor");
        _leftGrid = FindChildRT(panelRoot, "LeftGrid");
        _rightGrid = FindChildRT(panelRoot, "RightGrid");

        StretchToFullScreen(_root);

        if (_dimmer && closeOnBackground)
        {
            var bgBtn = _dimmer.GetComponent<Button>();
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

        int perSide = cols * rows;   // 4
        int maxTotal = perSide * 2;  // 8
        int total = Mathf.Clamp(options.Count > 0 ? options.Count : maxTotal, 1, maxTotal);

        for (int i = 0; i < total; i++)
        {
            var parent = (i < perSide) ? _leftGrid : _rightGrid;
            var btn = Instantiate(_runtimeButtonPrefab, parent);
            btn.gameObject.SetActive(true);

            // »çÀÌÁî º¸Á¤
            var rt = (RectTransform)btn.transform;
            if (rt.sizeDelta == Vector2.zero) rt.sizeDelta = new Vector2(cell, cell);

            // ½ÇÁ¦ ¿É¼ÇÀÌ ÀÖÀ¸¸é Bind, ¾øÀ¸¸é ºó ½½·Ô Ã³¸®
            var hasOpt = i < options.Count;
            if (hasOpt)
            {
                var opt = options[i];
                btn.Bind(opt, picked =>
                {
                    _onPick?.Invoke(picked); // ¿ÜºÎ ÄÝ¹é ½ÇÇà
                    Close();                 // UI ´Ý±â
                });
            }
            else
            {
                btn.Bind(default, null);
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
        foreach (var rt in root.GetComponentsInChildren<RectTransform>(true))
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
//buildUI.OpenAtWorld(map.CellCenterWorld(cell), options, picked =>
//{
//    // ºñ¿ë Ã¼Å©
//    // ½ÇÁ¦ Å¸¿ö ¹èÄ¡
//});
