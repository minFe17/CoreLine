using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class DefeatPanelControl : MonoBehaviour
{
    // 하이어라키 자동 배선
    private TMP_Text _stageNameText;
    private Button _retryButton;
    private Button _lobbyButton;

    // 옵션
    [SerializeField] private string _lobbySceneName = "LobyScene"; // 빌드 세팅에 추가되어 있어야 함
    [SerializeField] private bool _pauseOnShow = true;             // 패널 켤 때 일시정지
    [SerializeField] private bool _resumeOnExit = true;            // 나갈 때 timeScale 복구

    private void Awake()
    {
        Transform tStage = transform.Find("StageName");
        if (tStage) _stageNameText = tStage.GetComponent<TMP_Text>();

        Transform tRetry = transform.Find("RetryButton");
        if (tRetry) _retryButton = tRetry.GetComponent<Button>();

        Transform tLobby = transform.Find("LobbyButton");
        if (tLobby) _lobbyButton = tLobby.GetComponent<Button>();

        if (_retryButton)
        {
            _retryButton.onClick.RemoveAllListeners();
            _retryButton.onClick.AddListener(OnClickRetry);
        }
        if (_lobbyButton)
        {
            _lobbyButton.onClick.RemoveAllListeners();
            _lobbyButton.onClick.AddListener(OnClickLobby);
        }

        gameObject.SetActive(false);
    }

    public void Show(string stageId)
    {
        if (_stageNameText) _stageNameText.text = stageId;

        if (_pauseOnShow)
        {
            Time.timeScale = 0f;
            PauseControl.SetPaused(true);
        }

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // ── 버튼 핸들러 ─────────────────────────────────────

    private void OnClickRetry()
    {
        // 정지 해제
        if (_resumeOnExit)
        {
            Time.timeScale = 1f;
            PauseControl.SetPaused(false);
        }

        // 현재 씬 다시 로드
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    private void OnClickLobby()
    {
        // 정지 해제
        if (_resumeOnExit)
        {
            Time.timeScale = 1f;
            PauseControl.SetPaused(false);
        }

        if (string.IsNullOrEmpty(_lobbySceneName))
        {
            Debug.LogError("[DefeatPanelControl] 로비 씬 이름이 비었습니다.");
            return;
        }

        SceneManager.LoadScene(_lobbySceneName);
    }
}
