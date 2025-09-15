using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Utils;

[DisallowMultipleComponent]
public sealed class DefeatPanelControl : MonoBehaviour
{
    public event Action LobbyRequested;
    public event Action RetryRequested;

    private TMP_Text _stageNameText;
    private Button _retryButton;
    private Button _lobbyButton;

     private string _lobbySceneName = "LobbyScene";
    [SerializeField] private bool _pauseOnShow = true;
    [SerializeField] private bool _resumeOnExit = true;

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

    public void Hide() => gameObject.SetActive(false);

    private void OnClickRetry()
    {
        if (RetryRequested != null) { RetryRequested.Invoke(); return; }

        if (_resumeOnExit)
        {
            Time.timeScale = 1f;
            PauseControl.SetPaused(false);
        }
        GameManager.Instance.ResetRunState();
        var current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    private void OnClickLobby()
    {
        if (LobbyRequested != null) { LobbyRequested.Invoke(); return; }
        GameManager.Instance.ResetRunState(resetCostToZero: false);
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
