using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public class GameManager : MonoBehaviour
{
    [Header("스테이지 프리팹 (중클릭으로 로드 테스트)")]
    [SerializeField] private GameObject stagePrefab;

    [Header("UI 프리팹")]
    [SerializeField] private BuildUI buildUIPrefab;            // 빌드 선택 링 등
    [SerializeField] private TwoButtonUI twoButtonUIPrefab;    // 공용 2버튼 패널(취소/파괴 or 취소/발동)

    [SerializeField] private InvalidPlacementToast invalidPlacementPrefab; // 인스펙터에 연결
    [SerializeField] private GameObject playerBasePrefab;

    [Header("Test Skill Wallet Seed")]
    [SerializeField] private int startSkillCurrency = 0;       // 스킬 코인 초기값

    [Header("Auto-add test skills to loadout")]
    [SerializeField] private bool addTestSkills = true;
    [SerializeField] private int rangeHealCost = 10;
    [SerializeField] private float rangeHealValue = 50f;

    [Header("Clear Panel")]
    [SerializeField] private ClearPanelControl clearPanel;     // 인스펙터 배선
    [SerializeField] private RewardItemUI rewardItemPrefab;    // 인스펙터 배선(아이콘+수량 프리팹)
    [SerializeField] private string lobbySceneName = "LobyScene";

    private InvalidPlacementToast _invalidToast;

    private Camera _cam;
    private BuildUI _buildUI;
    private TwoButtonUI _panel;
    private Unit _unit;
    private UnitState _unitState;

    // ─────────────────────────────────────────────────────────────

    private void OnEnable()
    {
        if (NormalStageManager.Instance != null)
            NormalStageManager.Instance.StageCleared += OnStageCleared;

        // 패널 이벤트 구독
        if (clearPanel != null)
        {
            clearPanel.NextStageRequested += OnClickNextStage;
            clearPanel.LobbyRequested += OnClickLobbyFromClear;
        }

        StartCoroutine(CoWatchTimeout());
    }

    private void OnDisable()
    {
        if (NormalStageManager.Instance != null)
            NormalStageManager.Instance.StageCleared -= OnStageCleared;

        if (clearPanel != null)
        {
            clearPanel.NextStageRequested -= OnClickNextStage;
            clearPanel.LobbyRequested -= OnClickLobbyFromClear;
        }
    }

    private void Awake()
    {
       
        if (DataManager.Instance != null)
        {
            // 있으면 로드, 없으면 새로 생성하도록 DataManager가 내부에서 처리하게 설계
            DataManager.Instance.LoadData();    // 또는 EnsureLoaded()
        }
        // 스테이지 로드
        if (MapManager.Instance != null)
            MapManager.Instance.LoadStage(stagePrefab);

        SelectStageForCurrentPrefab();

        // 카메라/이벤트시스템/레이캐스터 보장
        _cam = Camera.main ?? FindFirstObjectByType<Camera>();
        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        if (_cam && _cam.GetComponent<Physics2DRaycaster>() == null)
            _cam.gameObject.AddComponent<Physics2DRaycaster>();

        // Canvas 확보
        var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null)
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // BuildUI 인스턴스
        _buildUI = FindFirstObjectByType<BuildUI>(FindObjectsInactive.Include);
        if (_buildUI == null && buildUIPrefab != null)
        {
            _buildUI = Instantiate(buildUIPrefab, canvas.transform);
            _buildUI.gameObject.name = "BuildUI(Runtime)";
            _buildUI.Close();
        }

        // TwoButtonUI 인스턴스
        _panel = FindFirstObjectByType<TwoButtonUI>(FindObjectsInactive.Include);
        if (_panel == null && twoButtonUIPrefab != null)
        {
            _panel = Instantiate(twoButtonUIPrefab, canvas.transform);
            _panel.gameObject.name = "TwoButtonUI(Runtime)";
            _panel.gameObject.SetActive(false);
        }
    }

    private void Start()
    {
        if (CostManager.Instance != null)
            CostManager.Instance.SetSkillValue(startSkillCurrency);

        // 테스트 스킬 3개 자동 추가
        if (addTestSkills && SkillManager.Instance != null && SkillManager.Instance._loadout.Count == 0)
        {
            SkillManager.Instance.AddToLoadout(new LaboratoryData
            {
                Id = "RangeHeal",
                Info = "광역 힐(테스트)",
                LaboratoryType = LaboratoryType.Defense,
                Cost = rangeHealCost,
                Effect = new Effect
                {
                    Value = rangeHealValue,
                    ValueType = ValueType.Add,
                    TargetType = TargetType.Unit,
                    TargetStatus = TargetStatus.HealthPoint
                },
                ParentsId = new List<string>()
            });

            SkillManager.Instance.AddToLoadout(new LaboratoryData
            {
                Id = "ArrowRain",
                Info = "광역 데미지(테스트)",
                LaboratoryType = LaboratoryType.Attack,
                Cost = 10,
                Effect = new Effect
                {
                    Value = 50,
                    ValueType = ValueType.Add,
                    TargetType = TargetType.Monster,
                    TargetStatus = TargetStatus.HealthPoint
                },
                ParentsId = new List<string>()
            });

            SkillManager.Instance.AddToLoadout(new LaboratoryData
            {
                Id = "MonsterSlow",
                Info = "광역 슬로우(테스트)",
                LaboratoryType = LaboratoryType.Utility,
                Cost = 12,
                Effect = new Effect
                {
                    Value = 50,
                    ValueType = ValueType.Add,
                    TargetType = TargetType.Monster,
                    TargetStatus = TargetStatus.AttackSpeed
                },
                ParentsId = new List<string>()
            });
        }
    }

    private void Update()
    {
        // 일시정지 중에는 입력 막기
        if (PauseControl.IsPaused) return;

        // 중클릭: 스테이지 리로드
        if (Input.GetMouseButtonDown(2))
        {
            if (stagePrefab)
            {
                MapManager.Instance.LoadStage(stagePrefab);
                Debug.Log("[GameManager] 스테이지 로드 완료.");
            }
            else Debug.LogError("[GameManager] stagePrefab이 비어있습니다.");
        }

        // 우클릭: 플레이어 베이스 선택
        if (Input.GetMouseButtonDown(1))
        {
            if (IsPointerOverUI()) return;

            var map = MapManager.Instance;
            if (!map || !map.IsReady) return;

            Vector3 world = _cam.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0f;
            Vector3Int cell = map.WorldToCell(world);

            bool placed = map.SelectPlayerBase(cell, playerBasePrefab, occupyBaseCell: true);
            if (placed) Debug.Log($"[GameManager] 플레이어 베이스 선택: {cell}");
            return;
        }

        // 좌클릭: 파괴/발동/빌드
        if (Input.GetMouseButtonDown(0))
        {
            // 1) ObjectTile '발동' 패널
            if (TryOpenObjectTilePanel()) return;

            // 2) UI 위 클릭 무시
            if (IsPointerOverUI()) return;

            // 3) 빌드/파괴
            var map = MapManager.Instance;
            if (!map || !map.IsReady) return;

            Vector3 world = _cam.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0f;
            Vector3Int cell = map.WorldToCell(world);

            // 파괴 가능한 벽
            if (map.IsDestructible(cell))
            {
                _panel.OpenAtCell(cell, "파괴", (id, payload) =>
                {
                    map.DestroyWallAt((Vector3Int)payload);
                });
                return;
            }

            // 일반 빌드 UI
            if (_buildUI == null) return;

            var infoAtClick = map.GetPlaceInfo(cell);

            // 빌드 불가 or 이미 점유
            if (!infoAtClick.Placeable || infoAtClick.Occupied)
            {
                PingInvalidAtCell(cell);
                return;
            }

            // 빌드 가능 → BuildUI 오픈
            var buildOptions = MakeOptionsForTest(8); // 실제 옵션으로 교체
            _buildUI.OpenAtCell(cell, buildOptions, (picked, selectedCell) =>
            {
                if (picked.Prefab == null) return;

                var info = map.GetPlaceInfo(selectedCell);
                if (!info.Placeable || info.Occupied)
                {
                    PingInvalidAtCell(selectedCell);
                    return;
                }

                Vector3 pos = map.CellCenterWorld(selectedCell);
                var go = Instantiate(picked.Prefab, pos, Quaternion.identity);
                map.RegisterTower(selectedCell, go);
            });
        }
    }

    // ───────── 타이머 종료 감시 ─────────
    private IEnumerator CoWatchTimeout()
    {
        while (true)
        {
            if (TimerPanelUI.IsTimeOverGlobal)
            {
                var mgr = NormalStageManager.Instance;
                if (mgr != null)
                {
                    // 선택된 스테이지 기준으로 필요한 값만 채운 스냅샷 구성
                    var stage = mgr.SelectedStage;
                    var snap = ConditionControl.BuildFor(stage);

                    mgr.CompleteStageSuccess(snap);
                }
                yield break; // 한 번만 처리
            }
            yield return null;
        }
    }


    // ───────── 클리어 시 UI 처리 ─────────
    private void OnStageCleared(NormalStageData stage,
                                NormalStageManager.StageEndSnapshot snap,
                                int stars,
                                NormalStageManager.RewardResult reward)
    {
        // 게임 정지
        Time.timeScale = 0f;
        PauseControl.SetPaused(true);

        // 패널 표시
        if (clearPanel != null)
        {
            clearPanel.gameObject.SetActive(true);
            clearPanel.SetStageId(stage.Id);
            clearPanel.ShowStars(stage, snap);

            // 보상 라인 빌드
            var rewards = NormalStageManager.Instance.GetRewardsForStage(stage.Id);
            var prefab = rewardItemPrefab ?? clearPanel.GetComponentInChildren<RewardItemUI>(true);
            if (prefab != null) clearPanel.BuildRewardsByIds(prefab, rewards);
        }
    }

    // ───────── 유틸 ─────────
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true; // 마우스
        for (int i = 0; i < Input.touchCount; i++)
            if (EventSystem.current.IsPointerOverGameObject(Input.touches[i].fingerId))
                return true; // 터치
        return false;
    }

    // ObjectTile 클릭 시 '발동' 패널 열기
    private bool TryOpenObjectTilePanel()
    {
        if (_cam == null || _panel == null) return false;

        Vector3 wp = _cam.ScreenToWorldPoint(Input.mousePosition);
        wp.z = 0f;

        Collider2D[] hits = Physics2D.OverlapPointAll((Vector2)wp, ~0);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D h = hits[i];
            if (h && h.TryGetComponent<ObjectTile>(out ObjectTile objectTile))
            {
                _panel.OpenAtObject(objectTile, "발동", (id, payload) =>
                {
                    if (payload is ObjectTile ot) ot.Activate();
                });
                return true;
            }
        }
        return false;
    }

    // 더미/테스트 옵션 생성
    private List<TowerOption> MakeOptionsForTest(int count)
    {
        var list = new List<TowerOption>(count);
        for (int i = 0; i < count; i++)
            list.Add(new TowerOption($"opt_{i + 1}", null, null, (i + 1) * 10));
        return list;
    }

    private Canvas FindOrCreateCanvas()
    {
        var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null)
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        return canvas;
    }

    private InvalidPlacementToast EnsureInvalidToast()
    {
        if (_invalidToast != null) return _invalidToast;

        if (invalidPlacementPrefab == null)
        {
            Debug.LogError("[GameManager] invalidPlacementPrefab 이 비어있습니다. 인스펙터에 프리팹을 넣어주세요.");
            return null;
        }

        var canvas = FindOrCreateCanvas();
        _invalidToast = Instantiate(invalidPlacementPrefab, canvas.transform);
        _invalidToast.gameObject.name = "InvalidPlacementToast(Runtime)";
        return _invalidToast;
    }

    // 셀 위에 토스트 띄우기
    private void PingInvalidAtCell(Vector3Int cell)
    {
        if (_invalidToast == null)
        {
            var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            _invalidToast = Instantiate(invalidPlacementPrefab, canvas.transform);
            _invalidToast.name = "InvalidPlacementToast(Runtime)";
        }

        _invalidToast.ShowAtCell(cell);
    }
    // ===== 클리어 시 패널 띄우는 기존 핸들러 유지 =====
    // OnStageCleared(...) 안에서 clearPanel.Show(...), BuildRewardsByIds(...) 호출은 그대로.

    // ===== 버튼 동작 구현 =====
    private void OnClickLobbyFromClear()
    {
        // 재개 후 로비로
        Time.timeScale = 1f;
        PauseControl.SetPaused(false);
        if (!string.IsNullOrEmpty(lobbySceneName))
            SceneManager.LoadScene(lobbySceneName);
    }

    private void OnClickNextStage()
    {
        var mgr = NormalStageManager.Instance;
        if (mgr == null) return;

        NormalStageData next;
        WorldStageData world;
        if (!mgr.TryGetNextStageFromSelected(out next, out world))
        {
            Debug.Log("[GameManager] 다음 스테이지가 없습니다.");
            return;
        }

        // 선택 스테이지 갱신(원하면 유지)
        EventManager.Instance?.Invoke<NormalStageData>("SelectStage", next);

        // 프리팹 로드 (Id == 프리팹 파일명)
        var prefab = StagePrefabResolver.LoadById(next.Id);
        if (prefab == null) return;

        // 재개 후 로드
        Time.timeScale = 1f;
        PauseControl.SetPaused(false);
        MapManager.Instance.LoadStage(prefab);
    }

    public static class StagePrefabResolver
    {
        public static GameObject LoadById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            var prefab = Resources.Load<GameObject>($"Stages/{id}");
            if (prefab == null)
                Debug.LogError($"[StagePrefabResolver] Resources/Stages/{id}.prefab 을 찾지 못했습니다.");
            return prefab;
        }
    }
    private void SelectStageForCurrentPrefab()
    {
        var mgr = NormalStageManager.Instance;
        if (mgr == null) return;

        // 프리팹 이름을 곧바로 스테이지 ID로 사용
        string id = stagePrefab != null ? stagePrefab.name : "Stage1-1";

        NormalStageData stage;
        WorldStageData world;
        if (!mgr.TryFindStageById(id, out stage, out world))
        {
            // 데이터 테이블에 없을 때는 더미 값으로라도 세팅 (UI용)
            stage = new NormalStageData
            {
                Id = id,
                Gold = 100,
                Gem = 50,
                Condition = new List<Condition>
            {
                new Condition{ ClearType = ClearType.MoneySave,  Info = "200원 남기기",       Value = 200 },
                new Condition{ ClearType = ClearType.HealthSave, Info = "베이스 HP 50% 이상", Value = 0.5f },
                new Condition{ ClearType = ClearType.UnitSave,   Info = "유닛 파괴 5 미만",  Value = 5 }
            }
            };
        }

        // NormalStageManager는 "SelectStage" 이벤트를 구독하고 있으니 이렇게 넘기면 SelectedStage가 세팅됨
        EventManager.Instance?.Invoke<NormalStageData>("SelectStage", stage);
        Debug.Log($"[GameManager] SelectedStage = {stage.Id}");
    }
}
