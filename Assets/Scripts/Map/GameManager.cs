using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

[DefaultExecutionOrder(-1000)]
public class GameManager : MonoBehaviour
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

    // ─────────────────────────────────────────────────────────────
    private void OnEnable()
    {
        if (NormalStageManager.Instance != null)
            NormalStageManager.Instance.StageCleared += OnStageCleared;

        _clearPanelRef = FindFirstObjectByType<ClearPanelControl>(FindObjectsInactive.Include);
        if (_clearPanelRef != null)
        {
            _clearPanelRef.NextStageRequested += OnClickNextStage;
            _clearPanelRef.LobbyRequested += OnClickLobbyFromClear;
        }

        StartCoroutine(CoWatchTimeout());
    }

    private void OnDisable()
    {
        if (NormalStageManager.Instance != null)
            NormalStageManager.Instance.StageCleared -= OnStageCleared;

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

            // TODO: 실제 빌드 옵션으로 교체
            var buildOptions = MakeOptionsForTest(8);
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

        // 로드 완료 이벤트 브로드캐스트 (페이로드: stageId)
        EventManager.Instance?.Invoke<string>(EVT_STAGE_LOADED, stageId);
    }

    // ───────── 타이머 종료 감시(클리어 트리거) ─────────
    private IEnumerator CoWatchTimeout()
    {
        while (true)
        {
            if (TimerPanelUI.IsTimeOverGlobal)
            {
                var mgr = NormalStageManager.Instance;
                if (mgr != null)
                {
                    var stage = mgr.SelectedStage;
                    var snap = ConditionControl.BuildFor(stage); // 조건에 필요한 필드만 채움
                    mgr.CompleteStageSuccess(snap);
                }
                yield break;
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
