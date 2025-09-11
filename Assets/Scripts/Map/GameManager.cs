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

    private const string LobbySceneName = "LobyScene";

    // 씬에 존재한다고 가정(없으면 기능 일부 생략)
    private Camera _cam;
    private BuildUI _buildUI;                // 빌드 선택 UI
    private TwoButtonUI _panel;              // 파괴/발동 2버튼
    private InvalidPlacementToast _invalidToast; // 불가 토스트
    private ClearPanelControl _clearPanelRef;    // 클리어 패널
    private DefeatPanelControl _defeatPanelRef;
    private bool _endingShown;
    // ─────────────────────────────────────────────────────────────
    private void OnEnable()
    {
        var nsm = NormalStageManager.Instance;
        if (nsm != null)
        {
            nsm.StageCleared += OnStageCleared;
            nsm.StageDefeated += OnStageDefeated;
        }

        _clearPanelRef = FindFirstObjectByType<ClearPanelControl>(FindObjectsInactive.Include);
        if (_clearPanelRef != null)
        {
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

        // 선택된 스테이지 기준으로 로드
        await LoadStageByCurrentSelection();
    }

    private void Update()
    {
        if (PauseControl.IsPaused) return;

        // 좌클릭만 유지
        if (Input.GetMouseButtonDown(0))
        {
            // 1) 오브젝트 발동 패널
            if (TryOpenObjectTilePanel()) return;

            // 2) UI 위 클릭 무시
            if (IsPointerOverUI()) return;

            // 3) 파괴/빌드
            var map = MapManager.Instance;
            if (!map || !map.IsReady || _cam == null) return;

            Vector3 world = _cam.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0f;
            Vector3Int cell = map.WorldToCell(world);

            // 파괴 가능한 벽이면 파괴 패널
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

            // 빌드
            if (_buildUI == null) return;

            var infoAtClick = map.GetPlaceInfo(cell);
            if (!infoAtClick.Placeable || infoAtClick.Occupied)
            {
                PingInvalidAtCell(cell);
                return;
            }
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

        // 로드 완료 이벤트 브로드캐스트 (페이로드: stageId)
        EventManager.Instance?.Invoke<string>(EVT_STAGE_LOADED, stageId);
    }

    // ───────── 타이머 종료 감시(클리어 트리거) ─────────
    

    // ───────── 클리어 시 UI 처리 ─────────
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
            panel.gameObject.SetActive(true);       // 먼저 켠 다음
            panel.SetStageId(stage.Id);
            panel.ShowStars(stage, snap);           // 코루틴 안전

            // 보상 라인 빌드
            var rewards = NormalStageManager.Instance.GetRewardsForStage(stage.Id);
            var rewardPrefab = panel.GetComponentInChildren<RewardItemUI>(true);
            if (rewardPrefab == null) rewardPrefab = Resources.Load<RewardItemUI>("Reward/Reward");
            if (rewardPrefab != null) panel.BuildRewardsByIds(rewardPrefab, rewards);

            // 버튼 이벤트 보장
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

        // 1) 게임 정지(패널에서도 다시 한 번 처리하지만, 안전하게)
        Time.timeScale = 0f;
        PauseControl.SetPaused(true);

        // 2) 코스트 정지(있으면)
        //CostManager.Instance?.SetEarningEnabled(false);

        // 3) 충돌 가능 UI 닫기(선택)
        FindFirstObjectByType<BuildUI>(FindObjectsInactive.Include)?.Close();
        FindFirstObjectByType<TwoButtonUI>(FindObjectsInactive.Include)?.Close();

        // 4) 패배 패널 표시
        var panel = _defeatPanelRef ?? FindFirstObjectByType<DefeatPanelControl>(FindObjectsInactive.Include);
        if (panel != null)
        {
            panel.Show(stage.Id);
            _defeatPanelRef = panel;
        }
        else
        {
            Debug.LogWarning("[GameManager] DefeatPanelControl을 찾지 못했습니다.");
            // (옵션) 패널이 없으면 바로 로비로 보낼 수도 있음
            // Time.timeScale = 1f;
            // PauseControl.SetPaused(false);
            // SceneManager.LoadScene("LobyScene");
        }
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

        if (!mgr.TryGetNextStageFromSelected(out var next, out _))
        {
            Debug.Log("[GameManager] 다음 스테이지가 없습니다.");
            return;
        }

        // 선택 스테이지 갱신
        EventManager.Instance?.Invoke<NormalStageData>("SelectStage", next);

        // 프리팹 로드
        GameObject prefab = await StagePrefabResolver.LoadById(next.Id);
        if (prefab == null) return;

        Time.timeScale = 1f;
        PauseControl.SetPaused(false);
        LoadStageWithEvent(prefab, next.Id);
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
        // 시간/일시정지 해제
        Time.timeScale = 1f;
        PauseControl.SetPaused(false);

        // 전투/중개 상태 정리
        SimpleSingleton<MapUnitManager>.Instance?.RestartGame();
        SimpleSingleton<MediatorManager>.Instance?.ClearAll();

        // 열려있는 패널/타겟팅/빌드UI 등 닫기(있으면)
        FindFirstObjectByType<BuildUI>(FindObjectsInactive.Include)?.Close();
        FindFirstObjectByType<TwoButtonUI>(FindObjectsInactive.Include)?.Close();
        FindFirstObjectByType<UnitUI>(FindObjectsInactive.Include)?.Close();
        var clear = FindFirstObjectByType<ClearPanelControl>(FindObjectsInactive.Include);
        if (clear) clear.gameObject.SetActive(false);
        var defeat = FindFirstObjectByType<DefeatPanelControl>(FindObjectsInactive.Include);
        if (defeat) defeat.gameObject.SetActive(false);

        // 타이머 정지+리셋 (설치 게이트를 사용한다면 시작은 베이스 설치 후 자동)
        var timer = FindFirstObjectByType<TimerPanelUI>(FindObjectsInactive.Include);
        if (timer)
        {
            timer.StopProgress();
            timer.RestartProgress(timer.TotalDuration);
        }

        // 코스트 초기화 (아래 CostManager 헬퍼 사용)
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

            // Addressables 사용 (키 == 스테이지 ID)
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
