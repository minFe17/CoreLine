using UnityEngine;

public class SkillTargetingController : MonoBehaviour
{
    [Header("Optional Preview Prefabs")]
    [SerializeField] private GameObject _rectPreviewPrefab;
    [SerializeField] private GameObject _radiusPreviewPrefab;

    [Header("Line Preview (프리팹 없을 때 자동 생성)")]
    [SerializeField] private float _lineWidth = 0.07f;
    [SerializeField] private int _circleSegments = 48;
    [SerializeField] private string _sortingLayerName = "UI";

    private int _slotIndex;
    private SkillManager.SelectedSkill _selectedSkill;
    private SkillTargetingSpec _spec;

    private GameObject _previewInstance;
    private LineRenderer _line;
    private SpriteRenderer[] _tints;
    private Camera _worldCam;

    private bool _dragDriven = false;

    // 공유 머티리얼(런타임 생성 최소화)
    private static Material sLineMat;

    // ─────────────────────────────────────────────────────────────────────
    // 진입점: UI 드래그
    // ─────────────────────────────────────────────────────────────────────
    public void StartTargetingDrag(int skillSlotIndex,
    in SkillManager.SelectedSkill skill,
    in SkillTargetingSpec targetingSpec,
    Vector2 initialScreenPos, 
    Camera eventCamera = null)
    {
        if (PauseControl.IsPaused) return;

        _dragDriven = true;
        _slotIndex = skillSlotIndex;
        _selectedSkill = skill;
        _spec = targetingSpec;

        if (_worldCam == null) _worldCam = Camera.main;

        // 컨트롤러를 먼저 활성화
        gameObject.SetActive(true);

        CreatePreview();
        UpdateAffordabilityTint();

        // 첫 위치를 즉시 반영
        UpdateDragScreenPosition(initialScreenPos, eventCamera);

        enabled = false; // 드래그 구동 모드
    }

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

    public void CancelFromUI() => Cancel();

    // ─────────────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ─────────────────────────────────────────────────────────────────────
    private void OnDisable()
    {
        CleanupPreview();
        _dragDriven = false;
    }

    // ─────────────────────────────────────────────────────────────────────
    // 프리뷰 생성/정리
    // ─────────────────────────────────────────────────────────────────────
    private void CreatePreview()
    {
        CleanupPreview();

        Vector3Int origin; Vector3Int size; Vector3 cellSize = Vector3.one;
        if (MapManager.Instance != null && MapManager.Instance.IsReady)
            MapManager.Instance.GetNavFrame(out origin, out size, out cellSize);

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
                pts[0] = new Vector3(-w / 2f, -h / 2f, 0);
                pts[1] = new Vector3(w / 2f, -h / 2f, 0);
                pts[2] = new Vector3(w / 2f, h / 2f, 0);
                pts[3] = new Vector3(-w / 2f, h / 2f, 0);
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
                float d = _spec.RadiusWorld * 2f;
                _previewInstance.transform.localScale = new Vector3(d, d, 1f);
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
                    _line.SetPosition(i, new Vector3(Mathf.Cos(t) * _spec.RadiusWorld,
                                                     Mathf.Sin(t) * _spec.RadiusWorld, 0f));
                }
            }
        }
        else
        {
            _previewInstance = new GameObject("PointPreviewRuntime");
        }

        // ▼ UI 드래그 방해 금지: 프리뷰 & 모든 자식을 Ignore Raycast 레이어로
        if (_previewInstance != null)
        {
            int ignore = LayerMask.NameToLayer("Ignore Raycast");
            if (ignore >= 0)
            {
                foreach (var tr in _previewInstance.GetComponentsInChildren<Transform>(true))
                    tr.gameObject.layer = ignore;
            }
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

        if (sLineMat == null)
            sLineMat = new Material(Shader.Find("Sprites/Default"));
        lr.material = sLineMat;

        lr.startColor = new Color(0f, 1f, 0f, 0.9f);
        lr.endColor = lr.startColor;

        if (!string.IsNullOrEmpty(_sortingLayerName))
        {
            lr.sortingLayerName = _sortingLayerName;
            lr.sortingOrder = 1000;
        }

        int ignore = LayerMask.NameToLayer("Ignore Raycast");
        if (ignore >= 0) lr.gameObject.layer = ignore;
    }

    // ─────────────────────────────────────────────────────────────────────
    // 위치/색상 갱신
    // ─────────────────────────────────────────────────────────────────────
    private void ApplyPreviewWorldPosition(Vector3 world)
    {
        if (_previewInstance == null) return;

        if (_spec.Mode == TargetingMode.RectCells &&
            MapManager.Instance != null && MapManager.Instance.IsReady)
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

    private bool CanAfford()
    {
        if (SkillManager.Instance != null &&
            SkillManager.Instance.TryGetDef(_selectedSkill.Id, out var def))
        {
            return CostManager.Instance == null || (CostManager.Instance.CurrentSkill >= def.Cost);
        }
        return true;
    }

    private void UpdateAffordabilityTint()
    {
        bool affordable = CanAfford();

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
                if (_tints[i] != null) _tints[i].color = tint;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // 시전/취소
    // ─────────────────────────────────────────────────────────────────────
    private void CommitAtWorld(Vector3 world)
    {
        if (PauseControl.IsPaused) return;
        if (!CanAfford()) return; // 코스트 부족하면 무시

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
            // 포인트형(단일 타겟 지정이 필요한 타입이면 외부에서 explicitTarget을 넘기도록 설계)
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
