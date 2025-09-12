using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-10)]
public class PauseUI : MonoBehaviour
{
    private const string NAME_PauseButton = "PauseButton";
    private const string NAME_PausePanel = "PausePanel";
    private const string NAME_PlayButton = "PlayButton";
    private const string NAME_RestartButton = "RestartButton";
    private const string NAME_QuitButton = "QuitButton";

    private const string LOBBY_SCENE_NAME = "LobyScene";
    private const KeyCode TOGGLE_KEY = KeyCode.Escape;

    private GameObject _pausePanel;
    private Button _pauseBtn, _playBtn, _restartBtn, _quitBtn;

    public static bool IsPaused { get; private set; }
    private bool _loadingScene; // 씬 전환 중 입력/토글 차단

    private void Awake()
    {
        // 기준 캔버스
        Canvas canvas = GetComponentInParent<Canvas>(true);
        if (!canvas) canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);

        // 이름으로 가져오기(비활성 포함)
        _pauseBtn = FindByName<Button>(canvas?.transform, NAME_PauseButton);
        _pausePanel = FindByName<Transform>(canvas?.transform, NAME_PausePanel)?.gameObject;
        _playBtn = FindByName<Button>(canvas?.transform, NAME_PlayButton);
        _restartBtn = FindByName<Button>(canvas?.transform, NAME_RestartButton);
        _quitBtn = FindByName<Button>(canvas?.transform, NAME_QuitButton);

        if (_pausePanel) _pausePanel.SetActive(false);

        // 이벤트 배선
        if (_pauseBtn) { _pauseBtn.onClick.RemoveAllListeners(); _pauseBtn.onClick.AddListener(TogglePause); }
        if (_playBtn) { _playBtn.onClick.RemoveAllListeners(); _playBtn.onClick.AddListener(Resume); }
        if (_restartBtn) { _restartBtn.onClick.RemoveAllListeners(); _restartBtn.onClick.AddListener(RestartScene); }
        if (_quitBtn)
        {
            _quitBtn.onClick.RemoveAllListeners();
            _quitBtn.onClick.AddListener(RequestDefeatAndShowPanel);
        }


#if UNITY_EDITOR
        if (!_pauseBtn) Debug.LogWarning("[PauseUI] PauseButton not found.");
        if (!_pausePanel) Debug.LogWarning("[PauseUI] PausePanel not found.");
        if (!_playBtn) Debug.LogWarning("[PauseUI] PlayButton not found.");
        if (!_restartBtn) Debug.LogWarning("[PauseUI] RestartButton not found.");
        if (!_quitBtn) Debug.LogWarning("[PauseUI] QuitButton not found.");
#endif

        // 씬 시작 시 항상 재생 상태 보정
        ForceResumeState();
    }

    private void OnEnable()
    {
        // 새 씬 로드 후에도 항상 보정
        SceneManager.sceneLoaded += EnsureUnpausedOnSceneLoaded;
        ForceResumeState();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= EnsureUnpausedOnSceneLoaded;
    }

    private void Update()
    {
        if (_loadingScene) return; // 전환 중 입력 무시
        if (Input.GetKeyDown(TOGGLE_KEY))
            TogglePause();
    }

    // ─────────────────────────────────────────────

    public void TogglePause()
    {
        if (_loadingScene) return;
        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (IsPaused || _loadingScene) return;
        IsPaused = true;
        if (_pausePanel) _pausePanel.SetActive(true);
        Time.timeScale = 0f;
        PauseControl.SetPaused(true);
        // AudioListener.pause = true;
    }

    public void Resume()
    {
        ForceResumeState();
    }

    private void ForceResumeState()
    {
        IsPaused = false;
        if (_pausePanel) _pausePanel.SetActive(false);
        Time.timeScale = 1f;
        PauseControl.SetPaused(false);
        // AudioListener.pause = false;
    }

    private void ForceHidePanelImmediate()
    {
        if (_pausePanel && _pausePanel.activeSelf)
            _pausePanel.SetActive(false);
    }

    public void RestartScene()
    {
        BeginSceneChange();

        MapManager.Instance?.UnloadStage(); // 선택
        var active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.buildIndex);
    }

    public void GoToLobby()
    {
        BeginSceneChange();
        GameManager.Instance?.ResetRunState();
        try
        {
            if (!string.IsNullOrEmpty(LOBBY_SCENE_NAME))
                SceneManager.LoadScene(LOBBY_SCENE_NAME);
            else
                SceneManager.LoadScene(0);
        }
        catch
        {
            SceneManager.LoadScene(0);
        }
    }
    private void RequestDefeatAndShowPanel()
    {
        // 일시정지 패널만 숨기고(씬 전환/리셋 X)
        ForceHidePanelImmediate();

        var timer = FindFirstObjectByType<TimerPanelUI>(FindObjectsInactive.Include);
        if (timer) timer.StopProgress();

        // 3) 포기로 패배 마감 → GameManager.OnStageDefeated가 패널을 띄움
        var nsm = NormalStageManager.Instance;
        if (nsm == null) return;

        var stage = nsm.SelectedStage;
        var snap = ConditionControl.BuildFor(stage);
        nsm.CompleteStageDefeat(snap); // 씬 전환 없음!
    }

    private void BeginSceneChange()
    {
        _loadingScene = true;        // 전환 중 입력/토글 차단
        ForceHidePanelImmediate();   // 깜빡임 방지
        ForceResumeState();          // 전환 전에 반드시 재생 상태

        GameManager.Instance?.ResetRunState(resetCostToZero: false);
    }

    private void EnsureUnpausedOnSceneLoaded(Scene s, LoadSceneMode m)
    {
        _loadingScene = false;
        ForceResumeState();
    }

    // ─────────────────────────────────────────────
    // 유틸: 이름으로 찾기
    private static T FindByName<T>(Transform root, string name) where T : Component
    {
        if (!root) return null;
        var list = root.GetComponentsInChildren<T>(true);
        for (int i = 0; i < list.Length; i++)
        {
            var c = list[i];
            if (c && c.name == name) return c;
        }
        return null;
    }
}
