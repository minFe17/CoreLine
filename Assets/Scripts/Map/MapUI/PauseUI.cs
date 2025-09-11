using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor;
using Utils;

[DefaultExecutionOrder(-10)]
public class PauseUI : MonoBehaviour
{
    private const string NAME_PauseButton = "PauseButton";
    private const string NAME_PausePanel = "PausePanel";
    private const string NAME_PlayButton = "PlayButton";
    private const string NAME_RestartButton = "RestartButton";
    private const string NAME_QuitButton = "QuitButton";
    
    private const string LOBBY_SCENE_NAME = "Lobby"; // 없으면 빌드 인덱스 0으로 이동
    private const KeyCode TOGGLE_KEY = KeyCode.Escape;
    
    private GameObject _pausePanel;
    private Button _pauseBtn, _playBtn, _restartBtn, _quitBtn;

    public static bool IsPaused { get; private set; }

    private void Awake()
    {
        // 기준 캔버스 탐색
        Canvas canvas = GetComponentInParent<Canvas>(true);
        if (!canvas) canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);

        // 이름으로 모두 찾기(비활성 포함)
        _pauseBtn = FindByName<Button>(canvas.transform, NAME_PauseButton);
        _pausePanel = FindByName<Transform>(canvas.transform, NAME_PausePanel)?.gameObject;
        _playBtn = FindByName<Button>(canvas.transform, NAME_PlayButton);
        _restartBtn = FindByName<Button>(canvas.transform, NAME_RestartButton);
        _quitBtn = FindByName<Button>(canvas.transform, NAME_QuitButton);

        // 초기 상태
        if (_pausePanel) _pausePanel.SetActive(false);

        // 클릭 이벤트 배선
        if (_pauseBtn)
        {
            _pauseBtn.onClick.RemoveAllListeners();
            _pauseBtn.onClick.AddListener(TogglePause);
        }
        if (_playBtn)
        {
            _playBtn.onClick.RemoveAllListeners();
            _playBtn.onClick.AddListener(Resume);
        }
        if (_restartBtn)
        {
            _restartBtn.onClick.RemoveAllListeners();
            _restartBtn.onClick.AddListener(RestartScene);
        }
        if (_quitBtn)
        {
            _quitBtn.onClick.RemoveAllListeners();
            _quitBtn.onClick.AddListener(GoToLobby);
        }

        // 누락된 요소가 있으면 경고만 띄움(실행은 계속)
        if (!_pauseBtn) Debug.LogWarning("[PauseUIAuto] PauseButton not found.");
        if (!_pausePanel) Debug.LogWarning("[PauseUIAuto] PausePanel not found.");
        if (!_playBtn) Debug.LogWarning("[PauseUIAuto] PlayButton not found.");
        if (!_restartBtn) Debug.LogWarning("[PauseUIAuto] RestartButton not found.");
        if (!_quitBtn) Debug.LogWarning("[PauseUIAuto] QuitButton not found.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(TOGGLE_KEY))
            TogglePause();
    }

    // ─────────────────────────────────────────────

    public void TogglePause()
    {
        if (IsPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        if (IsPaused) return;
        IsPaused = true;
        if (_pausePanel) _pausePanel.SetActive(true);
        Time.timeScale = 0f;
        PauseControl.SetPaused(true);
        //AudioListener.pause = true;
    }

    public void Resume()
    {
        if (!IsPaused) return;
        IsPaused = false;
        if (_pausePanel) _pausePanel.SetActive(false);
        Time.timeScale = 1f;
        PauseControl.SetPaused(false);
        //AudioListener.pause = false;
    }

    public void RestartScene()
    {
        Resume();
        SimpleSingleton<MapUnitManager>.Instance.RestartGame();
        SimpleSingleton<MediatorManager>.Instance.ClearAll();
        MapManager.Instance.UnloadStage();
        var active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.buildIndex);
        CostManager.Instance.SetUnitValue(0);
    }

    public void GoToLobby()
    {
        Resume();
        // 먼저 이름으로 시도, 없으면 인덱스 0
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

    // ─────────────────────────────────────────────
    // 유틸: 이름으로 찾기
    static T FindByName<T>(Transform root, string name) where T : Component
    {
        if (!root) return null;
        foreach (var c in root.GetComponentsInChildren<T>(true))
            if (c.name == name) return c;
        return null;
    }
    static Transform FindByName<Transform>(Transform root, string name) where Transform : Component
    {
        if (!root) return null;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t;
        return null;
    }
}
