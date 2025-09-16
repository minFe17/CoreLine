using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;
using System;
using System.Reflection;

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

    private const string TutorialClearedPrefsKey = "Tutorial_Cleared3Star";

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

        // ★ 사용자가 1-1을 골라도, 튜토리얼 미완료면 1-0으로 다시 보정
        CoerceSelectionForTutorialIfNeeded();

        // 선택된 스테이지 기준으로 로드
        await LoadStageByCurrentSelection();

        // 인풋 등 나머지 설정 (원본 유지)
        // ...
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

    // GameManager.cs
    // 튜토리얼 3별 여부 확인(필드/프로퍼티 모두 지원)
    private bool IsStageThreeStar(string stageId)
    {
        DataManager dataManager = DataManager.Instance;
        if (dataManager == null || dataManager.GameData == null || string.IsNullOrEmpty(stageId))
            return false;

        object gameData = dataManager.GameData;

        object clearStageListObject = GetMemberValue(gameData, "ClearStage"); // 필드/프로퍼티 케어
        System.Collections.IList list = clearStageListObject as System.Collections.IList;
        if (list == null) return false;

        for (int i = 0; i < list.Count; i++)
        {
            object item = list[i];
            if (item == null) continue;

            object idObject = GetMemberValue(item, "StageId");
            string idString = idObject as string;
            if (string.IsNullOrEmpty(idString) ||
                !string.Equals(idString, stageId, System.StringComparison.OrdinalIgnoreCase))
                continue;

            // 1) MaxStarNum으로 우선 판정
            object maxStarObject = GetMemberValue(item, "MaxStarNum");
            if (maxStarObject is int maxStar && maxStar >= 3) return true;

            // 2) Star 구조로 보조 판정
            object starObject = GetMemberValue(item, "Star");
            if (starObject != null)
            {
                bool first = GetBoolMember(starObject, "FirstStar");
                bool second = GetBoolMember(starObject, "SecondStar");
                bool third = GetBoolMember(starObject, "ThirdStar");
                if (first && second && third) return true;
            }

            return false; // 이 스테이지 항목은 찾았지만 3별은 아님
        }

        return false; // 목록에 해당 스테이지 항목이 없음
    }

    // ── 아래는 보조 유틸: 필드/프로퍼티 호환 ──
    private object GetMemberValue(object instance, string name)
    {
        if (instance == null) return null;
        System.Type type = instance.GetType();

        System.Reflection.PropertyInfo prop = type.GetProperty(name);
        if (prop != null) return prop.GetValue(instance, null);

        System.Reflection.FieldInfo field = type.GetField(name);
        if (field != null) return field.GetValue(instance);

        return null;
    }

    private void SetMemberValue(object instance, string name, object value)
    {
        if (instance == null) return;
        System.Type type = instance.GetType();

        System.Reflection.PropertyInfo prop = type.GetProperty(name);
        if (prop != null) { prop.SetValue(instance, value, null); return; }

        System.Reflection.FieldInfo field = type.GetField(name);
        if (field != null) { field.SetValue(instance, value); }
    }

    private bool GetBoolMember(object instance, string name)
    {
        object val = GetMemberValue(instance, name);
        return val is bool b && b;
    }


    private void SelectByIdIfFound(string stageId)
    {
        if (string.IsNullOrEmpty(stageId)) return;

        NormalStageManager normalStageManager = NormalStageManager.Instance;
        if (normalStageManager == null) return;

        NormalStageData foundStage;
        WorldStageData foundWorld; 

        if (normalStageManager.TryFindStageById(stageId, out foundStage, out foundWorld))
        {
            EventManager.Instance?.Invoke<NormalStageData>("SelectStage", foundStage);
            return;
        }

        if (useTutorialStage &&
            string.Equals(stageId, TutorialStageId, System.StringComparison.OrdinalIgnoreCase))
        {
            NormalStageData injected = BuildInjectedTutorialStage();

            // struct는 null 비교 불가 → Id가 비었는지로 유효성 판단
            if (!string.IsNullOrEmpty(injected.Id))
            {
                Debug.Log("[GameManager] Tutorial stage not found in data. Injecting runtime stage data.");
                EventManager.Instance?.Invoke<NormalStageData>("SelectStage", injected);
                return;
            }
        }

        Debug.LogWarning($"[GameManager] stageId '{stageId}' 을 WorldStageDatas에서 찾지 못했습니다.");
    }


    private NormalStageData BuildInjectedTutorialStage()
    {
        // 튜토리얼 스테이지(1-0) 런타임 주입용 최소 데이터
        NormalStageData stage = new NormalStageData();
        stage.Id = TutorialStageId;                 // "Stage1-0"
        stage.Name = "Stage 1-0";

        // 조건 1개 주입: MoneySave / "클리어하기" / Value=0
        List<Condition> conditions = new List<Condition>();

        Condition condition1 = new Condition();
        condition1.ClearType = ClearType.MoneySave;
        condition1.Info = "클리어하기";
        condition1.Value = 1f;
        conditions.Add(condition1);

        Condition condition2 = new Condition();
        condition2.ClearType = ClearType.MoneySave;
        condition2.Info = "클리어하기";
        condition2.Value = 1f;
        conditions.Add(condition2);

        Condition condition3 = new Condition();
        condition3.ClearType = ClearType.MoneySave;
        condition3.Info = "클리어하기";
        condition3.Value = 1f;
        conditions.Add(condition3);

        stage.Condition = conditions;

        stage.Gold = 0;
        stage.Gem = 0;
        stage.UnlockCharacter = null;

        return stage;
    }

    private void CoerceSelectionForTutorialIfNeeded()
    {
        if (!useTutorialStage) return;

        NormalStageManager normalStageManager = NormalStageManager.Instance;
        if (normalStageManager == null) return;

        // ★ 세션 플래그(또는 GameData 3★) 켜져 있으면 보정 중단
        if (IsTutorialClearedThisSession()) return;

        if (!string.IsNullOrEmpty(normalStageManager.SelectedStage.Id) &&
            string.Equals(normalStageManager.SelectedStage.Id, FirstNormalStageId, System.StringComparison.OrdinalIgnoreCase))
        {
            // 사용자가 1-1을 골랐지만 튜토리얼 미완료면 1-0으로 바꿔치기
            SelectByIdIfFound(TutorialStageId);
        }
    }


    // ───────── 데이터 기반 스테이지 로드 ─────────
    private async Task LoadStageByCurrentSelection()
    {
        NormalStageManager normalStageManager = NormalStageManager.Instance;
        if (normalStageManager == null)
        {
            Debug.LogError("[GameManager] NormalStageManager 가 없습니다.");
            return;
        }

        if (string.IsNullOrEmpty(normalStageManager.SelectedStage.Id))
        {
            Debug.LogError("[GameManager] SelectedStage.Id 가 비어있습니다. SelectStage 이벤트로 먼저 선택하세요.");
            return;
        }

        string stageIdToLoad = normalStageManager.SelectedStage.Id;

        // ★ 로드 직전에 한 번 더 보정하되, 세션 플래그를 먼저 본다
        if (useTutorialStage && !IsTutorialClearedThisSession())
        {
            if (string.Equals(stageIdToLoad, FirstNormalStageId, System.StringComparison.OrdinalIgnoreCase))
            {
                SelectByIdIfFound(TutorialStageId);
                stageIdToLoad = TutorialStageId;
            }
        }

        GameObject prefab = await StagePrefabResolver.LoadById(stageIdToLoad);
        if (prefab == null)
        {
            Debug.LogError($"[GameManager] 스테이지 프리팹 로드 실패: {stageIdToLoad}");
            return;
        }

        LoadStageWithEvent(prefab, normalStageManager.SelectedStage.Id);
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

        //시작하자마자 못 쓰게: CSV 쿨타임으로 채움
        SkillManager.Instance?.StartAllCooldownsFromDefs(useCsvCooltime: true);

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
        //튜토리얼(1-0)이라면, 설명 패널을 띄운 뒤 승리로 변환하도록 TutorialManager에 위임
        var tman = FindFirstObjectByType<TutorialManager>(FindObjectsInactive.Include);
        if (tman != null && tman.TryHandleDefeatAndConvertToWin(stage))
            return;

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
    private bool IsTutorialClearedThisSession()
    {
        // PlayerPrefs 플래그가 1이면 true
        if (PlayerPrefs.GetInt(TutorialClearedPrefsKey, 0) == 1)
            return true;

        // 보조: GameData에도 3★가 있으면 true
        return IsStageThreeStar(TutorialStageId);
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
