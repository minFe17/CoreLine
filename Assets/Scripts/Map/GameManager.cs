using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

[DefaultExecutionOrder(-1000)]
public class GameManager : MonoBehaviour
{
    [Header("스테이지 프리팹 (중클릭으로 로드 테스트)")]
    [SerializeField] private GameObject stagePrefab;

    [Header("UI 프리팹")]
    [SerializeField] private BuildUI buildUIPrefab;   // 빌드 선택 링 등
    [SerializeField] private TwoButtonUI twoButtonUIPrefab; // 공용 2버튼 패널(취소/파괴 or 취소/발동)

    [SerializeField] private InvalidPlacementToast invalidPlacementPrefab; // 인스펙터에 연결
    [SerializeField] private GameObject playerBasePrefab;

    [Header("Test Skill Wallet Seed")]
    [SerializeField] private int startSkillCurrency = 0;   // 스킬 코인 초기값

    [Header("Auto-add a test skill to loadout")]
    [SerializeField] private bool addRangeHeal = true;       // RangeHeal 자동 추가
    [SerializeField] private int rangeHealCost = 10;         // 테스트용 비용
    [SerializeField] private float rangeHealValue = 50f;     // 힐량

    private InvalidPlacementToast _invalidToast;

    private Camera _cam;
    private BuildUI _buildUI;
    private TwoButtonUI _panel;

    private void Awake()
    {
        // 스테이지 로드
        MapManager.Instance.LoadStage(stagePrefab);

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
        {
            CostManager.Instance.SetSkillValue(startSkillCurrency);
        }

        // 2) 로드아웃 비어있으면 테스트 스킬 강제 추가
        SkillManager.Instance.AddToLoadout(new LaboratoryData
        {
            Id = "RangeHeal",
            Info = "광역 힐(테스트)",
            LaboratoryType = LaboratoryType.Defense,
            Cost = rangeHealCost,
            Effect = new Effect
            {
                Value = 50,
                ValueType = ValueType.Add,
                TargetType = TargetType.Unit,                 // 타워/유닛 대상
                TargetStatus = TargetStatus.HealthPoint
            },
            ParentsId = new List<string>()
        });
        SkillManager.Instance.AddToLoadout(new LaboratoryData
        {
            Id = "ArrowRain",
            Info = "광역 뎀(테스트)",
            LaboratoryType = LaboratoryType.Attack,
            Cost = 10,
            Effect = new Effect
            {
                Value = 50,
                ValueType = ValueType.Add,
                TargetType = TargetType.Monster,                 // 타워/유닛 대상
                TargetStatus = TargetStatus.HealthPoint
            },
            ParentsId = new List<string>()
        });
        SkillManager.Instance.AddToLoadout(new LaboratoryData
        {
            Id = "MonsterSlow",
            Info = "광역 슬로우(테스트)",
            LaboratoryType = LaboratoryType.Defense,
            Cost = rangeHealCost,
            Effect = new Effect
            {
                Value = 50,
                ValueType = ValueType.Add,
                TargetType = TargetType.Monster,                 // 타워/유닛 대상
                TargetStatus = TargetStatus.HealthPoint
            },
            ParentsId = new List<string>()
        });
        Debug.Log("[GameManagerBoot] Added test skill 'RangeHeal' to loadout (slot 0).");
        
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { ShowClearPanelTest(); return; }

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
        if (Input.GetMouseButtonDown(1))
        {
            // UI 위면 무시
            if (IsPointerOverUI()) return;

            var map = MapManager.Instance;
            if (!map || !map.IsReady) return;

            Vector3 world = _cam.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0f;
            Vector3Int cell = map.WorldToCell(world);

            //킹타일이 아니면 false 반환됨
            bool placed = map.SelectPlayerBase(cell, playerBasePrefab /* null이면 오브젝트 생성 안함 */, occupyBaseCell: true);
            if (placed)
            {
                Debug.Log($"[GameManager] 플레이어 베이스 선택: {cell}");
                // 이후 하이라이터는 OnPlayerBasePlaced 이벤트로 킹 강조를 자동 종료함
            }
            else
            {
                // 킹타일이 아니면 아무것도 안 함(원하면 토스트/사운드 추가 가능)
                // PingInvalidAtCell(cell); // 필요하면 주석 해제
            }
            return; // 우클릭 동작 후 종료(좌클릭 로직과 분리)
        }
        // 좌클릭
        if (Input.GetMouseButtonDown(0))
        {
            // 1) 먼저 ObjectTile 클릭인지 검사 → 맞으면 '발동' 패널 열고 종료
            if (TryOpenObjectTilePanel()) return;

            // 2) UI 위 클릭이면 무시
            if (IsPointerOverUI()) return;

            // 3) 파괴/빌드 로직
            var map = MapManager.Instance;
            if (!map || !map.IsReady) return;

            Vector3 world = _cam.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0f;
            Vector3Int cell = map.WorldToCell(world);

            // 파괴 가능 벽이면 '파괴' 패널
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

            // 1) 빌드 불가면(또는 이미 점유) → UI 안 띄우고 토스트만
            if (!infoAtClick.Placeable || infoAtClick.Occupied)
            {
                // (주의) 파괴 가능 / 오브젝트 발동은 별도 분기로 이미 처리했다는 가정
                PingInvalidAtCell(cell);
                return;
            }

            // 2) 여기서부터 빌드 가능한 타일만 BuildUI 오픈
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

    // ───────── 유틸 ─────────
    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true; // 마우스
        for (int i = 0; i < Input.touchCount; i++)
            if (EventSystem.current.IsPointerOverGameObject(Input.touches[i].fingerId))
                return true; // 터치
        return false;
    }

    // ObjectTile 클릭 시 '발동' 패널 열기
    bool TryOpenObjectTilePanel()
    {
        if (_cam == null || _panel == null) return false;

        Vector3 wp = _cam.ScreenToWorldPoint(Input.mousePosition);
        wp.z = 0f;

        // 겹침 고려해서 전 레이어 검사
        Collider2D[] hits = Physics2D.OverlapPointAll((Vector2)wp, ~0);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D h = hits[i];
            if (h && h.TryGetComponent<ObjectTile>(out ObjectTile objectTile))
            {
                _panel.OpenAtObject(objectTile, "발동", (id, payload) =>
                {
                    if (payload is ObjectTile objectTile) objectTile.Activate();
                });
                return true;
            }
        }
        return false;
    }

    // 더미/테스트 옵션 생성
    List<TowerOption> MakeOptionsForTest(int count)
    {
        var list = new List<TowerOption>(count);
        for (int i = 0; i < count; i++)
            list.Add(new TowerOption($"opt_{i + 1}", null, null, (i + 1) * 10));
        return list;
    }
    Canvas FindOrCreateCanvas()
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
    InvalidPlacementToast EnsureInvalidToast()
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
    void PingInvalidAtCell(Vector3Int cell)
    {
        if (_invalidToast == null)
        {
            // 인스펙터에서 프리팹 할당했다면 Instantiate만 하고 꺼두지 마세요.
            var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            _invalidToast = Instantiate(invalidPlacementPrefab, canvas.transform);
            _invalidToast.name = "InvalidPlacementToast(Runtime)";
            // Awake에서 알아서 active=true + alpha=0 으로 준비됨
        }

        _invalidToast.ShowAtCell(cell);
    }
    private void ShowClearPanelTest()
    {
        // 1) 패널 찾기
        ClearPanelControl panel = FindFirstObjectByType<ClearPanelControl>(FindObjectsInactive.Include);
        if (panel == null)
        {
            Debug.LogWarning("[GameManager] ClearPanelControl 을 찾을 수 없습니다.");
            return;
        }

        // 2) 스테이지/스냅샷 준비 (선택된 스테이지가 없으면 더미 생성)
        NormalStageData stage = default(NormalStageData);
        NormalStageManager mgr = NormalStageManager.Instance;

        if (mgr != null && !string.IsNullOrEmpty(mgr.SelectedStage.Id))
        {
            stage = mgr.SelectedStage;
        }
        else
        {
            stage = new NormalStageData
            {
                Id = "Stage1-1",
                Gold = 100,
                Gem = 50,
                Condition = new List<Condition>
            {
                new Condition { ClearType = ClearType.MoneySave,  Info = "200원 남기기",       Value = 200 },
                new Condition { ClearType = ClearType.HealthSave, Info = "베이스 HP 50% 이상", Value = 0.5f },
                new Condition { ClearType = ClearType.UnitSave,   Info = "유닛 파괴 5 미만",  Value = 5 }
            }
            };
        }

        NormalStageManager.StageEndSnapshot snap = new NormalStageManager.StageEndSnapshot
        {
            moneyLeft = 210,
            baseHpRatio = 0.62f,
            unitDestroyedCount = 3
        };

        // 3) 패널 표시 (스테이지ID, 별 채우기)
        panel.gameObject.SetActive(true);
        panel.SetStageId(stage.Id);
        panel.ShowStars(stage, snap);

        // 4) 보상 생성 (아이콘은 매니저에서 ID로 찾음)
        List<(string id, int value)> rewards = new List<(string id, int value)>();
        if (mgr != null)
        {
            rewards = mgr.GetRewardsForStage(stage.Id);
            if (rewards == null || rewards.Count == 0)
            {
                rewards = new List<(string id, int value)> { ("Gold", stage.Gold), ("Gem", stage.Gem) };
            }
        }
        else
        {
            rewards = new List<(string id, int value)> { ("Gold", stage.Gold), ("Gem", stage.Gem) };
        }

        // Reward 프리팹 확보(우선 Resources → 없으면 씬 내 템플릿 복제)
        RewardItemUI rewardPrefab = Resources.Load<RewardItemUI>("Reward/Reward");
        if (rewardPrefab == null) rewardPrefab = Resources.Load<RewardItemUI>("Reward");
        if (rewardPrefab == null)
        {
            rewardPrefab = panel.GetComponentInChildren<RewardItemUI>(true);
            if (rewardPrefab == null)
            {
                Debug.LogWarning("[GameManager] RewardItemUI 프리팹을 찾을 수 없어 보상 표시는 생략합니다. (Resources/UI/RewardItem 권장)");
                return;
            }
        }

        panel.BuildRewardsByIds(rewardPrefab, rewards);
    }
}
