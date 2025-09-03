using UnityEngine;

public class SkillTargetingController : MonoBehaviour
{
    [Header("Optional Preview Prefabs")]
    [SerializeField] private GameObject _rectPreviewPrefab;
    [SerializeField] private GameObject _radiusPreviewPrefab;

    [Header("Line Preview (ÇÁ¸®ÆÕ ¾øÀ» ¶§ ÀÚµ¿ »ý¼º)")]
    [SerializeField] private float _lineWidth = 0.07f;
    [SerializeField] private int _circleSegments = 48;
    [SerializeField] private string _sortingLayerName = "UI"; // ÇÊ¿ä ¾øÀ¸¸é ºó ¹®ÀÚ¿­

    private int _slotIndex;
    private SkillManager.SelectedSkill _selectedSkill;
    private SkillTargetingSpec _spec;

    private GameObject _previewInstance;
    private LineRenderer _line;               // ¶óÀÎ ÇÁ¸®ºä¿ë
    private SpriteRenderer[] _tints;          // ÇÁ¸®ÆÕ ÇÁ¸®ºä »ö Æ¾Æ®
    private Camera _worldCam;

    // µå·¡±×-ÁÖµµ ¸ðµå ¿©ºÎ (UI µå·¡±×¿¡¼­ À§Ä¡ °»½ÅÀ» Á÷Á¢ È£Ãâ)
    private bool _dragDriven = false;

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÁøÀÔÁ¡ 1) ¸¶¿ì½º/Å°º¸µå ±â¹Ý (±âÁ¸ ¹æ½Ä)
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void StartTargeting(int skillSlotIndex, in SkillManager.SelectedSkill skill, in SkillTargetingSpec targetingSpec)
    {
        if (PauseControl.IsPaused) return;

        _dragDriven = false;

        _slotIndex = skillSlotIndex;
        _selectedSkill = skill;
        _spec = targetingSpec;

        if (_worldCam == null) _worldCam = Camera.main;

        CreatePreview();
        UpdatePreviewToMouse();
        UpdateAffordabilityTint();

        enabled = true;              // Update()¿¡¼­ ¸¶¿ì½º µû¶ó°¨
        gameObject.SetActive(true);
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÁøÀÔÁ¡ 2) UI µå·¡±× ±â¹Ý (µå·¡±× ½ÃÀÛ ½Ã È£Ãâ)
    //  - Update´Â »ç¿ëÇÏÁö ¾Ê°í, ¿ÜºÎ¿¡¼­ UpdateDragScreenPosition() À¸·Î ÁÂÇ¥ °»½Å
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    public void StartTargetingDrag(int skillSlotIndex, in SkillManager.SelectedSkill skill, in SkillTargetingSpec targetingSpec)
    {
        if (PauseControl.IsPaused) return;

        _dragDriven = true;

        _slotIndex = skillSlotIndex;
        _selectedSkill = skill;
        _spec = targetingSpec;

        if (_worldCam == null) _worldCam = Camera.main;

        CreatePreview();
        UpdateAffordabilityTint();

        enabled = false;             // ¿ÜºÎ°¡ À§Ä¡¸¦ ¹Ð¾î³Ö´Â ¸ðµå
        gameObject.SetActive(true);
    }

    // µå·¡±× Áß È­¸éÁÂÇ¥·Î ÇÁ¸®ºä À§Ä¡ °»½Å (UI EventSystem¿¡¼­ È£Ãâ)
    public void UpdateDragScreenPosition(Vector2 screenPos, Camera eventCamera = null)
    {
        if (PauseControl.IsPaused) return;
        if (_previewInstance == null) return;
        if (_worldCam == null) _worldCam = Camera.main;
        if (_worldCam == null) return;

        Vector3 world = _worldCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        world.z = 0f;
        ApplyPreviewWorldPosition(world);
        UpdateAffordabilityTint();
    }

    // µå·¡±× µå¶ø À§Ä¡·Î ½ÃÀü (UI EventSystem¿¡¼­ È£Ãâ)
    public void CommitFromScreen(Vector2 screenPos)
    {
        if (PauseControl.IsPaused) return;
        if (_worldCam == null) _worldCam = Camera.main;
        if (_worldCam == null) { CancelFromUI(); return; }

        Vector3 world = _worldCam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        world.z = 0f;
        CommitAtWorld(world);
        CancelFromUI();
    }

    // ¿ÜºÎ(UI)¿¡¼­ Ãë¼Ò
    public void CancelFromUI()
    {
        Cancel();
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // Unity lifecycle
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void OnDisable()
    {
        CleanupPreview();
        _dragDriven = false;
    }

    private void Update()
    {
        // µå·¡±×-ÁÖµµ ¸ðµå¿¡¼­´Â Update »ç¿ëÇÏÁö ¾ÊÀ½
        if (_dragDriven) return;
        if (PauseControl.IsPaused) { Cancel(); return; }
        if (_previewInstance == null) return;

        if (_worldCam == null)
        {
            _worldCam = Camera.main;
            if (_worldCam == null)
            {
                Debug.LogWarning("[SkillTargetingController] MainCamera not found.");
                return;
            }
        }

        UpdatePreviewToMouse();
        UpdateAffordabilityTint();

        // ¸¶¿ì½º/Å°º¸µå ½ÃÀü/Ãë¼Ò
        if (Input.GetMouseButtonUp(0))
        {
            Vector3 world = _worldCam.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0f;
            CommitAtWorld(world);
            Cancel();
        }
        else if (Input.GetMouseButtonUp(1) || Input.GetKeyUp(KeyCode.Escape))
        {
            Cancel();
        }
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÇÁ¸®ºä »ý¼º/Á¤¸®
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void CreatePreview()
    {
        CleanupPreview();

        // ¸Ê/¼¿ ½ºÄÉÀÏ È®º¸
        Vector3Int originCell;
        Vector3Int sizeCells;
        Vector3 cellSize = Vector3.one;
        if (MapManager.Instance != null && MapManager.Instance.IsReady)
        {
            MapManager.Instance.GetNavFrame(out originCell, out sizeCells, out cellSize);
        }

        if (_spec.Mode == TargetingMode.RectCells)
        {
            if (_rectPreviewPrefab != null)
            {
                _previewInstance = Instantiate(_rectPreviewPrefab);
                _tints = _previewInstance.GetComponentsInChildren<SpriteRenderer>(true);

                int cells = _spec.HalfSizeCells * 2 + 1;
                _previewInstance.transform.localScale = new Vector3(cellSize.x * cells, cellSize.y * cells, 1f);
            }
            else
            {
                _previewInstance = new GameObject("RectPreviewRuntime");
                _line = _previewInstance.AddComponent<LineRenderer>();
                SetupLine(_line);

                int cells = _spec.HalfSizeCells * 2 + 1;
                float w = cellSize.x * cells;
                float h = cellSize.y * cells;

                Vector3[] pts = new Vector3[5];
                pts[0] = new Vector3(-w / 2f, -h / 2f, 0f);
                pts[1] = new Vector3(w / 2f, -h / 2f, 0f);
                pts[2] = new Vector3(w / 2f, h / 2f, 0f);
                pts[3] = new Vector3(-w / 2f, h / 2f, 0f);
                pts[4] = pts[0];

                _line.positionCount = pts.Length;
                _line.SetPositions(pts);
            }
        }
        else if (_spec.Mode == TargetingMode.RadiusWorld)
        {
            if (_radiusPreviewPrefab != null)
            {
                _previewInstance = Instantiate(_radiusPreviewPrefab);
                float diameter = _spec.RadiusWorld * 2f;
                _previewInstance.transform.localScale = new Vector3(diameter, diameter, 1f);
                _tints = _previewInstance.GetComponentsInChildren<SpriteRenderer>(true);
            }
            else
            {
                _previewInstance = new GameObject("CirclePreviewRuntime");
                _line = _previewInstance.AddComponent<LineRenderer>();
                SetupLine(_line);

                _line.positionCount = _circleSegments + 1;
                for (int i = 0; i <= _circleSegments; i++)
                {
                    float t = (float)i / _circleSegments * Mathf.PI * 2f;
                    float x = Mathf.Cos(t) * _spec.RadiusWorld;
                    float y = Mathf.Sin(t) * _spec.RadiusWorld;
                    _line.SetPosition(i, new Vector3(x, y, 0f));
                }
            }
        }
        else
        {
            _previewInstance = new GameObject("PointPreviewRuntime");
        }
    }

    private void CleanupPreview()
    {
        if (_previewInstance != null) Destroy(_previewInstance);
        _previewInstance = null;
        _line = null;
        _tints = null;
    }

    private void SetupLine(LineRenderer lr)
    {
        lr.useWorldSpace = false;
        lr.widthMultiplier = _lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(0f, 1f, 0f, 0.9f);
        lr.endColor = lr.startColor;
        if (!string.IsNullOrEmpty(_sortingLayerName))
        {
            lr.sortingLayerName = _sortingLayerName;
            lr.sortingOrder = 1000;
        }
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ÇÁ¸®ºä À§Ä¡/»ö»ó °»½Å
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void UpdatePreviewToMouse()
    {
        if (_worldCam == null) return;
        Vector3 world = _worldCam.ScreenToWorldPoint(Input.mousePosition);
        world.z = 0f;
        ApplyPreviewWorldPosition(world);
    }

    private void ApplyPreviewWorldPosition(Vector3 world)
    {
        if (_spec.Mode == TargetingMode.RectCells && MapManager.Instance != null && MapManager.Instance.IsReady)
        {
            Vector3Int cell = MapManager.Instance.WorldToCell(world);
            Vector3 snapped = MapManager.Instance.CellCenterWorld(cell);
            _previewInstance.transform.position = snapped;
        }
        else
        {
            _previewInstance.transform.position = world;
        }
    }

    private void UpdateAffordabilityTint()
    {
        bool affordable = CostManager.Instance == null || (CostManager.Instance.CurrentSkill >= _selectedSkill.Cost);

        if (_line != null)
        {
            Color c = affordable ? new Color(0f, 1f, 0f, 0.9f) : new Color(1f, 0f, 0f, 0.9f);
            _line.startColor = c;
            _line.endColor = c;
        }

        if (_tints != null)
        {
            Color tint = affordable ? new Color(0f, 1f, 0f, 0.35f) : new Color(1f, 0f, 0f, 0.35f);
            for (int i = 0; i < _tints.Length; i++)
            {
                if (_tints[i] == null) continue;
                _tints[i].color = tint;
            }
        }
    }

    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    // ½ÃÀü/Ãë¼Ò
    // ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
    private void CommitAtWorld(Vector3 world)
    {
        if (PauseControl.IsPaused) return;
        if (_spec.Mode == TargetingMode.RectCells)
        {
            SkillManager.Instance.UseSkillAreaRectWorld(_slotIndex, world, _spec.HalfSizeCells, _spec.ValidTargets);
        }
        else if (_spec.Mode == TargetingMode.RadiusWorld)
        {
            SkillManager.Instance.UseSkillAreaRadiusWorld(_slotIndex, world, _spec.RadiusWorld, _spec.ValidTargets);
        }
        else
        {
            SkillManager.Instance.UseSkill(_slotIndex, null);
        }
    }

    private void Cancel()
    {
        CleanupPreview();
        enabled = false;
        gameObject.SetActive(false);
    }
}
