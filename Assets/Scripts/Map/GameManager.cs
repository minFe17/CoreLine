using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

[DefaultExecutionOrder(-1000)]
public class GameManager : MonoSingleton<GameManager>
{
    // ─────────────────────────────────────────────────────────────
    // 외부에서 쓰기 편하도록 공개 상수/플래그
    // ─────────────────────────────────────────────────────────────
    public const string EVT_STAGE_LOADED = "StageLoaded"; // EventManager 키
    public static bool IsStageLoaded { get; private set; } // 현재 스테이지 로드 완료 여부
    public static string LastLoadedStageId { get; private set; } // 마지막 로드된 스테이지 ID

    private const string LobbySceneName = "LobbyScene";

    // 튜토리얼 스테이지 부팅 설정
    [Header("Tutorial Stage Boot")]
    [SerializeField] private bool useTutorialStage = true;          // 튜토리얼을 별도 스테이지로 운용할지
    [SerializeField] private string TutorialStageId = "Stage1-0";   // 데이터/어드레서블 ID (튜토리얼)
    [SerializeField] private string FirstNormalStageId = "Stage1-1"; // 튜토리얼 다음 메인 스테이지

    // 씬에 존재한다고 가정(없으면 기능 일부 생략)
    private Camera _cam;
    private BuildUI _buildUI;                // 빌드 선택 UI
    private TwoButtonUI _panel;              // 파괴/발동 2버튼
    private InvalidPlacementToast _invalidToast; // 불가 토스트
    private ClearPanelControl _clearPanelRef;    // 클리어 패널
    private DefeatPanelControl _defeatPanelRef;  // 패배 패널
    private bool _endingShown;                  // 승/패 UI 중복 방지

    // ─────────────────────────────────────────────────────────────
    private void OnEnable()
    {
        var nsm = NormalStageManager.Instance;
        if (nsm != null)
        {
            nsm.StageCleared += OnStageCleared;
            nsm.StageDefeated += OnStageDefeated;
        }

        _defeatPanelRef = FindFirstObjectByType<DefeatPanelControl>(FindObjectsInactive.Include);
        if (_defeatPanelRef != null)
        {
            _defeatPanelRef.LobbyRequested -= OnClickLobbyFromDefeat;
            _defeatPanelRef.LobbyRequested += OnClickLobbyFromDefeat;
            _defeatPanelRef.RetryRequested -= OnClickRetryFromDefeat;
            _defeatPanelRef.RetryRequested += OnClickRetryFromDefeat;
        }

        _clearPanelRef = FindFirstObjectByType<ClearPanelControl>(FindObjectsInactive.Include);
        if (_clearPanelRef != null)
        {
            _clearPanelRef.NextStageRequested -= OnClickNextStage;
            _clearPanelRef.LobbyRequested -= OnClickLobbyFromClear;
            _clearPanelRef.NextStageRequested += OnClickNextStage;
            _clearPanelRef.LobbyRequested += OnClickLobbyFromClear;
        }
    }

    private void OnDisable()
    {
        var nsm = NormalStageManager.Instance;
        if (nsm != null)
        {
            nsm.StageCleared -= OnStageCleared;
            nsm.StageDefeated -= OnStageDefeated;
        }

        if (_defeatPanelRef != null)
        {
            _defeatPanelRef.LobbyRequested -= OnClickLobbyFromDefeat;
            _defeatPanelRef.RetryRequested -= OnClickRetryFromDefeat;
        }
        if (_clearPanelRef != null)
        {
            _clearPanelRef.NextStageRequested -= OnClickNextStage;
            _clearPanelRef.LobbyRequested -= OnClickLobbyFromClear;
        }
    }

    private async void Awake()
    {
        // 데이터 로드
        DataManager.Instance?.LoadData();

        // 카메라/이벤트 보장
        _cam = Camera.main ?? FindFirstObjectByType<Camera>();
        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        if (_cam && _cam.GetComponent<Physics2DRaycaster>() == null)
            _cam.gameObject.AddComponent<Physics2DRaycaster>();

        // UI 찾기
        _buildUI = FindFirstObjectByType<BuildUI>(FindObjectsInactive.Include);
        _panel = FindFirstObjectByType<TwoButtonUI>(FindObjectsInactive.Include);
        _invalidToast = FindFirstObjectByType<InvalidPlacementToast>(FindObjectsInactive.Include);
        _clearPanelRef = FindFirstObjectByType<ClearPanelControl>(FindObjectsInactive.Include);

        // ★ 선택이 비어있다면 튜토리얼/1-1 중 하나로 자동 선택 (튜토리얼은 3★ 완료 전까지 강제)
        EnsureInitialSelection();

        // 선택된 스테이지 기준으로 로드
        await LoadStageByCurrentSelection();
    }

    private void Update()
    {
        if (PauseControl.IsPaused) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (TryOpenObjectTilePanel()) return;
            if (IsPointerOverUI()) return;

            var map = MapManager.Instance;
            if (!map || !map.IsReady || _cam == null) return;

            Vector3 world = _cam.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0f;
            Vector3Int cell = map.WorldToCell(world);

            // 파괴
            if (map.IsDestructible(cell))
            {
                if (_panel != null)
                {
                    _panel.OpenAtCell(cell, "파괴", (id, payload) =>
                    {
                        map.DestroyWallAt((Vector3Int)payload);
                    });
                }
                return;
            }

            // 빌드 위치 유효성
            if (_buildUI == null) return;
            var infoAtClick = map.GetPlaceInfo(cell);
            if (!infoAtClick.Placeable || infoAtClick.Occupied)
            {
                PingInvalidAtCell(cell);
                return;
            }
        }
    }

    // ───────── 튜토리얼 선택 보정(3★ 전까지 강제) ─────────
    private void EnsureInitialSelection()
    {
        var nsm = NormalStageManager.Instance;
        if (nsm == null) return;

        // 이미 외부에서 SelectStage로 골라져 있다면 건드리지 않음
        if (!string.IsNullOrEmpty(nsm.SelectedStage.Id)) return;

        if (!useTutorialStage)
        {
            SelectByIdIfFound(FirstNormalStageId);
            return;
        }

        bool tutorialDone = IsStageThreeStar(TutorialStageId);
        if (!tutorialDone) SelectByIdIfFound(TutorialStageId);
        else SelectByIdIfFound(FirstNormalStageId);
    }

    private bool IsStageThreeStar(string stageId)
    {
        var gd = DataManager.Instance?.GameData;
        if (gd?.ClearStage == null) return false;

        for (int i = 0; i < gd.ClearStage.Count; i++)
        {
            var cs = gd.ClearStage[i];
            if (cs.StageId == stageId && cs.MaxStarNum >= 3) return true;
        }
        return false;
    }

    private void SelectByIdIfFound(string stageId)
    {
        if (string.IsNullOrEmpty(stageId)) return;

        var nsm = NormalStageManager.Instance;
        if (nsm.TryFindStageById(stageId, out var stage, out _))
        {
            EventManager.Instance?.Invoke<NormalStageData>("SelectStage", stage);
        }
        else
        {
            Debug.LogWarning($"[GameManager] stageId '{stageId}' 을 WorldStageDatas에서 찾지 못했습니다.");
        }
    }

    // ───────── 데이터 기반 스테이지 로드 ─────────
    private async Task LoadStageByCurrentSelection()
    {
        var mgr = NormalStageManager.Instance;
        if (mgr == null)
        {
            Debug.LogError("[GameManager] NormalStageManager 가 없습니다.");
            return;
        }

        if (string.IsNullOrEmpty(mgr.SelectedStage.Id))
        {
            Debug.LogError("[GameManager] SelectedStage.Id 가 비어있습니다. SelectStage 이벤트로 먼저 선택하세요.");
            return;
        }

        GameObject prefab = await StagePrefabResolver.LoadById(mgr.SelectedStage.Id);
        if (prefab == null)
        {
            Debug.LogError($"[GameManager] 스테이지 프리팹 로드 실패: {mgr.SelectedStage.Id}");
            return;
        }

        LoadStageWithEvent(prefab, mgr.SelectedStage.Id);
    }

    private void LoadStageWithEvent(GameObject prefab, string stageId)
    {
        StartCoroutine(CoLoadStageAndNotify(prefab, stageId));
    }

    private IEnumerator CoLoadStageAndNotify(GameObject prefab, string stageId)
    {
        IsStageLoaded = false;
        MapManager.Instance?.LoadStage(prefab);
        yield return null;
        yield return new WaitUntil(() => MapManager.Instance != null && MapManager.Instance.IsReady);

        IsStageLoaded = true;
        LastLoadedStageId = stageId;
        _endingShown = false;

        EventManager.Instance?.Invoke<string>(EVT_STAGE_LOADED, stageId);
    }

    // ───────── 클리어/패배 UI 처리 ─────────
    private void OnStageCleared(NormalStageData stage,
                                NormalStageManager.StageEndSnapshot snap,
                                int stars,
                                NormalStageManager.RewardResult reward)
    {
        if (_endingShown) return;
        _endingShown = true;

        Time.timeScale = 0f;
        PauseControl.SetPaused(true);

        var panel = _clearPanelRef ?? FindFirstObjectByType<ClearPanelControl>(FindObjectsInactive.Include);
        if (panel != null)
        {
            panel.gameObject.SetActive(true);
            panel.SetStageId(stage.Id);
            panel.ShowStars(stage, snap);

            var rewards = NormalStageManager.Instance.GetRewardsForStage(stage.Id);
            var rewardPrefab = panel.GetComponentInChildren<RewardItemUI>(true);
            if (rewardPrefab == null) rewardPrefab = Resources.Load<RewardItemUI>("Reward/Reward");
            if (rewardPrefab != null) panel.BuildRewardsByIds(rewardPrefab, rewards);

            panel.NextStageRequested -= OnClickNextStage;
            panel.LobbyRequested -= OnClickLobbyFromClear;
            panel.NextStageRequested += OnClickNextStage;
            panel.LobbyRequested += OnClickLobbyFromClear;

            _clearPanelRef = panel;
        }
        else
        {
            Debug.LogWarning("[GameManager] ClearPanelControl 을 찾지 못했습니다.");
        }
    }

    private void OnStageDefeated(NormalStageData stage,
                                 NormalStageManager.StageEndSnapshot snap,
                                 int stars,
                                 NormalStageManager.RewardResult reward)
    {
        if (_endingShown) return;
        _endingShown = true;

        var nsm = NormalStageManager.Instance;
        if (nsm != null)
        {
            nsm.StageCleared -= OnStageCleared;
            nsm.StageDefeated -= OnStageDefeated;
        }

        Time.timeScale = 0f;
        PauseControl.SetPaused(true);

        FindFirstObjectByType<BuildUI>(FindObjectsInactive.Include)?.Close();
        FindFirstObjectByType<TwoButtonUI>(FindObjectsInactive.Include)?.Close();

        var panel = _defeatPanelRef ?? FindFirstObjectByType<DefeatPanelControl>(FindObjectsInactive.Include);
        if (panel != null)
        {
            panel.Show(stage.Id);
            _defeatPanelRef = panel;
        }
        else
        {
            Debug.LogWarning("[GameManager] DefeatPanelControl을 찾지 못했습니다.");
        }
    }

    // ───────── 보스 사망 알림(유닛/보스 스크립트에서 호출) ─────────
    public void NotifyBossDead()
    {
        if (_endingShown) return;

        var nsm = NormalStageManager.Instance;
        if (nsm == null) return;

        var stage = nsm.SelectedStage;
        var snap = ConditionControl.BuildFor(stage);
        nsm.CompleteStageSuccess(snap); // → OnStageCleared에서 승리 패널 표시
    }

    // ───────── 버튼 동작 ─────────
    private void OnClickLobbyFromClear()
    {
        Time.timeScale = 1f;
        PauseControl.SetPaused(false);
        if (!string.IsNullOrEmpty(LobbySceneName))
            SceneManager.LoadScene(LobbySceneName);
    }

    private async void OnClickNextStage()
    {
        var mgr = NormalStageManager.Instance;
        if (mgr == null) return;

        // 튜토리얼이면 무조건 FirstNormalStageId로
        if (useTutorialStage &&
            !string.IsNullOrEmpty(mgr.SelectedStage.Id) &&
            string.Equals(mgr.SelectedStage.Id, TutorialStageId, System.StringComparison.OrdinalIgnoreCase))
        {
            if (mgr.TryFindStageById(FirstNormalStageId, out var first, out _))
            {
                EventManager.Instance?.Invoke<NormalStageData>("SelectStage", first);

                GameObject prefabA = await StagePrefabResolver.LoadById(first.Id);
                if (prefabA == null) return;

                Time.timeScale = 1f;
                PauseControl.SetPaused(false);
                LoadStageWithEvent(prefabA, first.Id);
                return;
            }
        }

        // 일반 흐름(월드 내 다음 → 다음 월드 첫 스테이지)
        if (!mgr.TryGetNextStageFromSelected(out var next, out _))
        {
            Debug.Log("[GameManager] 다음 스테이지가 없습니다.");
            return;
        }

        EventManager.Instance?.Invoke<NormalStageData>("SelectStage", next);
        GameObject prefab = await StagePrefabResolver.LoadById(next.Id);
        if (prefab == null) return;

        Time.timeScale = 1f;
        PauseControl.SetPaused(false);
        LoadStageWithEvent(prefab, next.Id);
    }

    private void OnClickLobbyFromDefeat()
    {
        Time.timeScale = 1f;
        PauseControl.SetPaused(false);
        ResetRunState(resetCostToZero: false);
        if (!string.IsNullOrEmpty(LobbySceneName))
            SceneManager.LoadScene(LobbySceneName);
        else
            SceneManager.LoadScene(0);
    }

    private void OnClickRetryFromDefeat()
    {
        Time.timeScale = 1f;
        PauseControl.SetPaused(false);
        ResetRunState(resetCostToZero: false);
        var active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.buildIndex);
    }

    // ───────── 유틸 ─────────
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true;
        for (int i = 0; i < Input.touchCount; i++)
            if (EventSystem.current.IsPointerOverGameObject(Input.touches[i].fingerId))
                return true;
        return false;
    }

    private bool TryOpenObjectTilePanel()
    {
        if (_cam == null || _panel == null) return false;

        Vector3 wp = _cam.ScreenToWorldPoint(Input.mousePosition);
        wp.z = 0f;

        Collider2D[] hits = Physics2D.OverlapPointAll((Vector2)wp, ~0);
        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
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

    private List<TowerOption> MakeOptionsForTest(int count)
    {
        var list = new List<TowerOption>(count);
        for (int i = 0; i < count; i++)
            list.Add(new TowerOption($"opt_{i + 1}", null, null, (i + 1) * 10));
        return list;
    }

    private void PingInvalidAtCell(Vector3Int cell)
    {
        if (_invalidToast == null)
            _invalidToast = FindFirstObjectByType<InvalidPlacementToast>(FindObjectsInactive.Include);

        if (_invalidToast != null)
            _invalidToast.ShowAtCell(cell);
    }

    public void ResetRunState(bool resetCostToZero = true)
    {
        Time.timeScale = 1f;
        PauseControl.SetPaused(false);

        SimpleSingleton<MapUnitManager>.Instance?.RestartGame();
        SimpleSingleton<MediatorManager>.Instance?.ClearAll();
        MapManager.Instance?.UnloadStage();

        FindFirstObjectByType<BuildUI>(FindObjectsInactive.Include)?.Close();
        FindFirstObjectByType<TwoButtonUI>(FindObjectsInactive.Include)?.Close();
        FindFirstObjectByType<UnitUI>(FindObjectsInactive.Include)?.Close();
        var clear = FindFirstObjectByType<ClearPanelControl>(FindObjectsInactive.Include);
        if (clear) clear.gameObject.SetActive(false);
        var defeat = FindFirstObjectByType<DefeatPanelControl>(FindObjectsInactive.Include);
        if (defeat) defeat.gameObject.SetActive(false);

        var timer = FindFirstObjectByType<TimerPanelUI>(FindObjectsInactive.Include);
        if (timer)
        {
            timer.StopProgress();
            timer.RestartProgress(timer.TotalDuration);
        }

        if (resetCostToZero)
            CostManager.Instance?.PrepareForNewStage(resetToZero: true);
        else
            CostManager.Instance?.PrepareForNewStage(resetToZero: false);
    }

    // ───────── 프리팹 로더 ─────────
    public static class StagePrefabResolver
    {
        public static async Task<GameObject> LoadById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            var aa = SimpleSingleton<AddressableManager>.Instance;
            GameObject prefab = aa != null
                ? await aa.GetAddressableAsset<GameObject>(id)
                : null;

            if (prefab == null)
                Debug.LogError($"[StagePrefabResolver] Addressables 에서 '{id}' 프리팹을 찾지 못했습니다.");
            return prefab;
        }
    }
}
