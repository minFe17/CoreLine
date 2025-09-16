using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utils;

[DefaultExecutionOrder(-1000)]
public class GameManager : MonoSingleton<GameManager>
{
    public const string EVT_STAGE_LOADED = "StageLoaded";
    public static bool IsStageLoaded { get; private set; }
    public static string LastLoadedStageId { get; private set; }

    private const string LobbySceneName = "LobbyScene";

    [Header("Tutorial Stage Boot")]
    [SerializeField] private bool useTutorialStage = true;
    [SerializeField] private string TutorialStageId = "Stage1-0";
    [SerializeField] private string FirstNormalStageId = "Stage1-1";

    private const string TutorialClearedPrefsKey = "Tutorial_Cleared3Star";

    private Camera _cam;
    private BuildUI _buildUI;
    private TwoButtonUI _panel;
    private InvalidPlacementToast _invalidToast;
    private ClearPanelControl _clearPanelRef;
    private DefeatPanelControl _defeatPanelRef;
    private bool _endingShown;

    private void OnEnable()
    {
        NormalStageManager nsm = NormalStageManager.Instance;
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
        NormalStageManager nsm = NormalStageManager.Instance;
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
        if (DataManager.Instance != null)
            DataManager.Instance.LoadData();

        _cam = Camera.main != null ? Camera.main : FindFirstObjectByType<Camera>();
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
        if (_cam != null && _cam.GetComponent<Physics2DRaycaster>() == null)
            _cam.gameObject.AddComponent<Physics2DRaycaster>();

        _buildUI = FindFirstObjectByType<BuildUI>(FindObjectsInactive.Include);
        _panel = FindFirstObjectByType<TwoButtonUI>(FindObjectsInactive.Include);
        _invalidToast = FindFirstObjectByType<InvalidPlacementToast>(FindObjectsInactive.Include);
        _clearPanelRef = FindFirstObjectByType<ClearPanelControl>(FindObjectsInactive.Include);

        EnsureInitialSelection();
        CoerceSelectionForTutorialIfNeeded();

        await LoadStageByCurrentSelection();   // 씬 리로드 후에도 여기서 선택된 스테이지를 로드한다.
    }

    private void Update()
    {
        if (PauseControl.IsPaused) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (TryOpenObjectTilePanel()) return;
            if (IsPointerOverUI()) return;

            MapManager map = MapManager.Instance;
            if (!map || !map.IsReady || _cam == null) return;

            Vector3 world = _cam.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0f;
            Vector3Int cell = map.WorldToCell(world);

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

            if (_buildUI == null) return;
            MapManager.PlaceInfo infoAtClick = map.GetPlaceInfo(cell);
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
        NormalStageManager nsm = NormalStageManager.Instance;
        if (nsm == null) return;
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
        DataManager dataManager = DataManager.Instance;
        if (dataManager == null || dataManager.GameData == null || string.IsNullOrEmpty(stageId))
            return false;

        object gameData = dataManager.GameData;

        object clearStageListObject = GetMemberValue(gameData, "ClearStage");
        System.Collections.IList list = clearStageListObject as System.Collections.IList;
        if (list == null) return false;

        for (int i = 0; i < list.Count; i++)
        {
            object item = list[i];
            if (item == null) continue;

            object idObject = GetMemberValue(item, "StageId");
            string idString = idObject as string;
            if (string.IsNullOrEmpty(idString) ||
                !string.Equals(idString, stageId, StringComparison.OrdinalIgnoreCase))
                continue;

            object maxStarObject = GetMemberValue(item, "MaxStarNum");
            if (maxStarObject is int maxStar && maxStar >= 3) return true;

            object starObject = GetMemberValue(item, "Star");
            if (starObject != null)
            {
                bool first = GetBoolMember(starObject, "FirstStar");
                bool second = GetBoolMember(starObject, "SecondStar");
                bool third = GetBoolMember(starObject, "ThirdStar");
                if (first && second && third) return true;
            }
            return false;
        }
        return false;
    }

    private object GetMemberValue(object instance, string name)
    {
        if (instance == null) return null;
        Type type = instance.GetType();

        PropertyInfo prop = type.GetProperty(name);
        if (prop != null) return prop.GetValue(instance, null);

        FieldInfo field = type.GetField(name);
        if (field != null) return field.GetValue(instance);

        return null;
    }
    private void SetMemberValue(object instance, string name, object value)
    {
        if (instance == null) return;
        Type type = instance.GetType();

        PropertyInfo prop = type.GetProperty(name);
        if (prop != null) { prop.SetValue(instance, value, null); return; }
        FieldInfo field = type.GetField(name);
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
            string.Equals(stageId, TutorialStageId, StringComparison.OrdinalIgnoreCase))
        {
            NormalStageData injected = BuildInjectedTutorialStage();
            if (!string.IsNullOrEmpty(injected.Id))
            {
                Debug.Log("[GameManager] Tutorial stage not found in data. Injecting runtime stage data.");
                EventManager.Instance?.Invoke<NormalStageData>("SelectStage", injected);
                return;
            }
        }

        Debug.LogWarning("[GameManager] stageId '" + stageId + "' 을 WorldStageDatas에서 찾지 못했습니다.");
    }

    private NormalStageData BuildInjectedTutorialStage()
    {
        NormalStageData stage = new NormalStageData();
        stage.Id = TutorialStageId;
        stage.Name = "Stage 1-0";

        List<Condition> conditions = new List<Condition>();
        for (int i = 0; i < 3; i++)
        {
            Condition c = new Condition();
            c.ClearType = ClearType.MoneySave;
            c.Info = "클리어하기";
            c.Value = 0f;
            conditions.Add(c);
        }
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

        if (IsTutorialClearedThisSession()) return;

        if (!string.IsNullOrEmpty(normalStageManager.SelectedStage.Id) &&
            string.Equals(normalStageManager.SelectedStage.Id, FirstNormalStageId, StringComparison.OrdinalIgnoreCase))
        {
            SelectByIdIfFound(TutorialStageId);
        }
    }

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

        if (useTutorialStage && !IsTutorialClearedThisSession())
        {
            if (string.Equals(stageIdToLoad, FirstNormalStageId, StringComparison.OrdinalIgnoreCase))
            {
                SelectByIdIfFound(TutorialStageId);
                stageIdToLoad = TutorialStageId;
            }
        }

        GameObject prefab = await StagePrefabResolver.LoadById(stageIdToLoad);
        if (prefab == null)
        {
            Debug.LogError("[GameManager] 스테이지 프리팹 로드 실패: " + stageIdToLoad);
            return;
        }

        LoadStageWithEvent(prefab, normalStageManager.SelectedStage.Id);
    }

    private void LoadStageWithEvent(GameObject stagePrefab, string stageId)
    {
        Debug.Log("[GameManager] LoadStageWithEvent: " + stageId);
        ResetRunState(resetCostToZero: true);
        StartCoroutine(CoLoadStageAndNotify(stagePrefab, stageId));
    }

    private IEnumerator CoLoadStageAndNotify(GameObject prefab, string stageId)
    {
        IsStageLoaded = false;

        if (MapManager.Instance != null) MapManager.Instance.LoadStage(prefab);
        yield return null;
        yield return new WaitUntil(() => MapManager.Instance != null && MapManager.Instance.IsReady);

        if (SkillManager.Instance != null)
            SkillManager.Instance.StartAllCooldownsFromDefs(useCsvCooltime: true);

        IsStageLoaded = true;
        LastLoadedStageId = stageId;
        _endingShown = false;

        EventManager.Instance?.Invoke<string>(EVT_STAGE_LOADED, stageId);
        Debug.Log("[GM] StageLoaded 발행: " + stageId);
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

        ClearPanelControl panel = _clearPanelRef != null
            ? _clearPanelRef
            : FindFirstObjectByType<ClearPanelControl>(FindObjectsInactive.Include);

        if (panel != null)
        {
            panel.gameObject.SetActive(true);
            panel.SetStageId(stage.Id);
            panel.ShowStars(stage, snap);

            List<(string id, int value)> rewards = NormalStageManager.Instance.GetRewardsForStage(stage.Id);

            RewardItemUI rewardPrefab = panel.GetComponentInChildren<RewardItemUI>(true);
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
        TutorialManager tman = FindFirstObjectByType<TutorialManager>(FindObjectsInactive.Include);
        if (tman != null && tman.TryHandleDefeatAndConvertToWin(stage))
            return;

        if (_endingShown) return;
        _endingShown = true;

        NormalStageManager nsm = NormalStageManager.Instance;
        if (nsm != null)
        {
            nsm.StageCleared -= OnStageCleared;
            nsm.StageDefeated -= OnStageDefeated;
        }

        Time.timeScale = 0f;
        PauseControl.SetPaused(true);

        BuildUI bui = FindFirstObjectByType<BuildUI>(FindObjectsInactive.Include);
        if (bui != null) bui.Close();
        TwoButtonUI tbu = FindFirstObjectByType<TwoButtonUI>(FindObjectsInactive.Include);
        if (tbu != null) tbu.Close();

        DefeatPanelControl panel = _defeatPanelRef != null
            ? _defeatPanelRef
            : FindFirstObjectByType<DefeatPanelControl>(FindObjectsInactive.Include);
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

    public void NotifyBossDead()
    {
        if (_endingShown) return;

        NormalStageManager nsm = NormalStageManager.Instance;
        if (nsm == null) return;

        NormalStageData stage = nsm.SelectedStage;
        NormalStageManager.StageEndSnapshot snap = ConditionControl.BuildFor(stage);
        nsm.CompleteStageSuccess(snap);
    }

    private void OnClickLobbyFromClear()
    {
        Time.timeScale = 1f;
        PauseControl.SetPaused(false);
        if (!string.IsNullOrEmpty(LobbySceneName))
            SceneManager.LoadScene(LobbySceneName);
    }

    private async void OnClickNextStage()
    {
        Debug.Log("[GameManager] OnClickNextStage 진입");

        var nsm = NormalStageManager.Instance;
        if (nsm == null) { Debug.LogError("[GameManager] NormalStageManager 가 null."); return; }

        string currentStageId = _clearPanelRef != null ? _clearPanelRef.GetCurrentStageId() : nsm.SelectedStage.Id;
        if (string.IsNullOrEmpty(currentStageId)) currentStageId = nsm.SelectedStage.Id;

        Time.timeScale = 1f;
        PauseControl.SetPaused(false);

        // ★ 다음 스테이지 '선택'만 하고 → 씬 리로드 (PauseUI의 Retry 흐름)
        if (useTutorialStage &&
            !string.IsNullOrEmpty(currentStageId) &&
            string.Equals(currentStageId, TutorialStageId, StringComparison.OrdinalIgnoreCase))
        {
            if (nsm.TryFindStageById(FirstNormalStageId, out var first, out _))
            {
                EventManager.Instance?.Invoke<NormalStageData>("SelectStage", first);
                _clearPanelRef?.Hide();
                RestartScene();                // ★ 씬 리로드로 전환
                return;
            }
            Debug.LogWarning("[GameManager] FirstNormalStageId('" + FirstNormalStageId + "') not found.");
            return;
        }

        if (!nsm.TryGetNextStageFromSelected(out var next, out _))
        {
            Debug.LogWarning("[GameManager] 다음 스테이지가 없습니다.");
            return;
        }

        EventManager.Instance?.Invoke<NormalStageData>("SelectStage", next);
        _clearPanelRef?.Hide();
        RestartScene();                        // ★ 씬 리로드로 전환
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
        Scene active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.buildIndex);
    }

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
            Collider2D h = hits[i];
            if (h != null && h.TryGetComponent<ObjectTile>(out ObjectTile objectTile))
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
        List<TowerOption> list = new List<TowerOption>(count);
        for (int i = 0; i < count; i++)
            list.Add(new TowerOption("opt_" + (i + 1), null, null, (i + 1) * 10));
        return list;
    }

    private void PingInvalidAtCell(Vector3Int cell)
    {
        if (_invalidToast == null)
            _invalidToast = FindFirstObjectByType<InvalidPlacementToast>(FindObjectsInactive.Include);

        if (_invalidToast != null)
            _invalidToast.ShowAtCell(cell);
    }

    // ───────── PauseUI 'Retry' 스타일 전환 유틸 ─────────
    // ★ 씬 리로드
    private void RestartScene()
    {
        BeginSceneChange();
        MapManager.Instance?.UnloadStage(); // 선택적으로 정리(안전)
        var active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.buildIndex);
    }

    // ★ 로비 이동
    private void GoToLobby()
    {
        BeginSceneChange();
        try
        {
            if (!string.IsNullOrEmpty(LobbySceneName))
                SceneManager.LoadScene(LobbySceneName);
            else
                SceneManager.LoadScene(0);
        }
        catch
        {
            SceneManager.LoadScene(0);
        }
    }

    // ★ 전환 전 공통 정리
    private void BeginSceneChange()
    {
        Time.timeScale = 1f;
        PauseControl.SetPaused(false);
        ResetRunState(resetCostToZero: false); // 런타임 정리만, 코스트는 0으로 강제하지 않음
    }

    // ───────── 표준 초기화 ─────────
    public void ResetRunState(bool resetCostToZero = true)
    {
        Debug.Log("[GM] ResetRunState 호출");
        StopWaveSystems();
        ForceClearUnits();

        Time.timeScale = 1f;
        PauseControl.SetPaused(false);

        MapManager.Instance?.UnloadStage();

        TryInvokeResetOnSingleton("EnemySpawner", new[] { "StopAll", "Abort", "ResetAll", "ClearAll" });
        TryInvokeResetOnSingleton("MonsterManager", new[] { "ClearAll", "DespawnAll", "ResetAll", "StopAndClear" });

        SimpleSingleton<MapUnitManager>.Instance?.RestartGame();
        SimpleSingleton<MediatorManager>.Instance?.ClearAll();

        BuildUI buildUi = FindFirstObjectByType<BuildUI>(FindObjectsInactive.Include);
        if (buildUi != null) buildUi.Close();
        TwoButtonUI twoButtonUi = FindFirstObjectByType<TwoButtonUI>(FindObjectsInactive.Include);
        if (twoButtonUi != null) twoButtonUi.Close();
        UnitUI unitUi = FindFirstObjectByType<UnitUI>(FindObjectsInactive.Include);
        if (unitUi != null) unitUi.Close();

        ClearPanelControl clear = FindFirstObjectByType<ClearPanelControl>(FindObjectsInactive.Include);
        if (clear != null) clear.gameObject.SetActive(false);
        DefeatPanelControl defeat = FindFirstObjectByType<DefeatPanelControl>(FindObjectsInactive.Include);
        if (defeat != null) defeat.gameObject.SetActive(false);

        TimerPanelUI timer = FindFirstObjectByType<TimerPanelUI>(FindObjectsInactive.Include);
        if (timer != null)
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
        if (PlayerPrefs.GetInt(TutorialClearedPrefsKey, 0) == 1)
            return true;
        return IsStageThreeStar(TutorialStageId);
    }

    // 웨이브/몬스터 정지 보조 --------------------------
    private void StopWaveSystems()
    {
        TryCallNoArg("EnemySpawner", "StopAll", "Abort", "ResetAll", "ClearAll");
        TryCallNoArg("WaveManager", "StopAll", "ResetAll", "ClearAll");
    }
    private void ForceClearUnits()
    {
        var units = FindObjectsByType<Unit>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < units.Length; i++)
        {
            try { units[i].Remove(); } catch { }
        }
    }

    // 리플렉션 유틸 --------------------------
    private void TryInvokeResetOnSingleton(string typeSimpleName, string[] candidateMethods)
    {
        try
        {
            var t = FindTypeBySimpleName(typeSimpleName);
            if (t == null) return;

            object instance = null;
            var p1 = t.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (p1 != null) instance = p1.GetValue(null, null);

            if (instance == null)
            {
                var unityObj = UnityEngine.Object.FindFirstObjectByType(t) as UnityEngine.Object;
                if (unityObj != null) instance = unityObj;
            }
            if (instance == null) return;

            foreach (var m in candidateMethods)
            {
                var mi = t.GetMethod(m, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (mi != null && mi.GetParameters().Length == 0)
                {
                    mi.Invoke(instance, null);
                    Debug.Log($"[GM] {typeSimpleName}.{m}() 호출");
                    return;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[GM] TryInvokeResetOnSingleton 예외: " + ex.Message);
        }
    }

    private Type FindTypeBySimpleName(string name)
    {
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try { types = asm.GetTypes(); } catch { continue; }
            for (int i = 0; i < types.Length; i++)
                if (types[i].Name == name) return types[i];
        }
        return null;
    }

    private void TryCallNoArg(string typeSimple, params string[] methods)
    {
        var t = FindTypeBySimpleName(typeSimple);
        if (t == null) return;

        object inst = null;
        var p = t.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null) inst = p.GetValue(null, null);
        if (inst == null)
        {
            var obj = FindFirstObjectByType(t) as UnityEngine.Object;
            if (obj != null) inst = obj;
        }
        if (inst == null) return;

        foreach (var m in methods)
        {
            var mi = t.GetMethod(m, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mi != null && mi.GetParameters().Length == 0)
            {
                try { mi.Invoke(inst, null); Debug.Log($"[GM] {typeSimple}.{m}() 호출"); }
                catch { }
            }
        }
    }

    // 프리팹 로더 --------------------------
    public static class StagePrefabResolver
    {
        public static async Task<GameObject> LoadById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            AddressableManager aa = SimpleSingleton<AddressableManager>.Instance;
            GameObject prefab = aa != null
                ? await aa.GetAddressableAsset<GameObject>(id)
                : null;

            if (prefab == null)
                Debug.LogError("[StagePrefabResolver] Addressables 에서 '" + id + "' 프리팁을 찾지 못했습니다.");
            return prefab;
        }
    }
}
