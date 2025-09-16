using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ClearPanelControl : MonoBehaviour
{
    // ----- config -----
    private float _starSpacing = 200f;                 // -200 / 0 / +200
    private float _starYBetweenCenterAndTop = 0.72f;   // 0.5=중앙, 1.0=최상단
    private bool _autoRepositionOnResize = true;
    private float _rewardSpacing = 200f;               // 보상 간격

    // Resources 경로
    private const string _starPrefabPath = "Reward/ClearStar"; // Resources/Reward/ClearStar.prefab

    // ----- wired (자동 배선) -----
    private RectTransform _panelRoot;
    private TMP_Text _stageNameText;
    private RectTransform _starGroup;

    private RectTransform _rewardBack;
    private HorizontalLayoutGroup _rewardHLG;
    private ContentSizeFitter _rewardCSF;
    private RectTransform _rewardsRoot;

    // StarCondition: 텍스트/아이콘
    private Transform _condRoot;
    private TMP_Text _firstCondText, _secondCondText, _thirdCondText;
    private GameObject _firstCondIcon, _secondCondIcon, _thirdCondIcon;

    // Buttons
    private Button _nextButton;         // ClearPanel/NextStageButton
    private Button _lobbyButton;        // ClearPanel/ClearLobbyButton

    // 이벤트(외부에서 구독)
    public event Action NextStageRequested;
    public event Action LobbyRequested;

    // 현재 패널이 표시 중인 스테이지ID 저장
    private string _currentStageId;

    // 수집된 StarUI
    private readonly List<StarUI> _stars = new List<StarUI>(4);
    private float _starStepWait = 0.55f;
    private bool _starSeqPlaying = false;

    private void Awake()
    {
        AutoWire();
        EnsureStars();      // 없으면 3개 생성
        RefreshStars();     // StarPos 밑 StarUI 자동 수집
        CenterRewards(_rewardSpacing); // 보상 줄 가운데+간격 세팅
        gameObject.SetActive(false);   // 기본은 숨김
    }

    private void OnEnable()
    {
        if (_autoRepositionOnResize) RepositionStars();
        StartCoroutine(CoRepositionAfterLayout());
    }

    private IEnumerator CoRepositionAfterLayout()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_panelRoot);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_starGroup);
        yield return null;
        Canvas.ForceUpdateCanvases();
        RepositionStars();
    }

    private void OnRectTransformDimensionsChange()
    {
        if (_autoRepositionOnResize && isActiveAndEnabled) RepositionStars();
    }

    // ClearPanelControl.cs
    private void AutoWire()
    {
        _panelRoot = GetComponent<RectTransform>();

        Transform transformStageName = transform.Find("StageName");
        if (transformStageName != null)
            _stageNameText = transformStageName.GetComponent<TMP_Text>();

        Transform transformStarPos = transform.Find("StarPos");
        if (transformStarPos != null)
            _starGroup = transformStarPos.GetComponent<RectTransform>();

        Transform transformRewardBack = transform.Find("RewardBack");
        if (transformRewardBack != null)
        {
            _rewardBack = transformRewardBack.GetComponent<RectTransform>();

            // Rewards 컨테이너 확보(없으면 생성) → 필드명은 _rewardsRoot 사용
            Transform transformRewards = _rewardBack.Find("Rewards");
            if (transformRewards == null)
            {
                GameObject rewardsObject = new GameObject("Rewards", typeof(RectTransform));
                RectTransform rewardsRectTransform = rewardsObject.GetComponent<RectTransform>();
                rewardsRectTransform.SetParent(_rewardBack, false);
                rewardsRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rewardsRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rewardsRectTransform.pivot = new Vector2(0.5f, 0.5f);
                _rewardsRoot = rewardsRectTransform;
            }
            else
            {
                _rewardsRoot = transformRewards.GetComponent<RectTransform>();
            }

            // 선언해 둔 레이아웃 컴포넌트들도 여기서 확보(없으면 추가)
            if (_rewardsRoot != null)
            {
                _rewardHLG = _rewardsRoot.GetComponent<HorizontalLayoutGroup>();
                if (_rewardHLG == null)
                    _rewardHLG = _rewardsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();

                _rewardCSF = _rewardsRoot.GetComponent<ContentSizeFitter>();
                if (_rewardCSF == null)
                    _rewardCSF = _rewardsRoot.gameObject.AddComponent<ContentSizeFitter>();

                _rewardCSF.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                _rewardCSF.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }

        // ----- Buttons -----
        Button nextButton = null;
        Button lobbyButton = null;

        Transform transformNext = transform.Find("NextStageButton");
        if (transformNext != null) nextButton = transformNext.GetComponent<Button>();

        Transform transformLobby = transform.Find("ClearLobbyButton");
        if (transformLobby != null) lobbyButton = transformLobby.GetComponent<Button>();

        // 이름이 다를 수 있으니 폴백(모든 자식 버튼 중에서 키워드로 탐색)
        if (nextButton == null || lobbyButton == null)
        {
            Button[] allButtons = GetComponentsInChildren<Button>(true);
            for (int i = 0; i < allButtons.Length; i++)
            {
                Button button = allButtons[i];
                if (button == null || button.transform == null) continue;

                string objectName = button.transform.name;
                if (nextButton == null &&
                    !string.IsNullOrEmpty(objectName) &&
                    (objectName.IndexOf("Next", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                     objectName.IndexOf("NextStage", System.StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    nextButton = button;
                }
                else if (lobbyButton == null &&
                         !string.IsNullOrEmpty(objectName) &&
                         (objectName.IndexOf("Lobby", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                          objectName.IndexOf("Home", System.StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    lobbyButton = button;
                }
            }
        }

        _nextButton = nextButton;
        _lobbyButton = lobbyButton;

        if (_nextButton != null)
        {
            _nextButton.onClick.RemoveAllListeners();
            _nextButton.onClick.AddListener(OnClickNext);
        }
        else
        {
            Debug.LogWarning("[ClearPanel] Next 버튼을 찾지 못했습니다. 이름을 'NextStageButton'으로 하거나 'Next'가 포함되도록 해 주세요.");
        }

        if (_lobbyButton != null)
        {
            _lobbyButton.onClick.RemoveAllListeners();
            _lobbyButton.onClick.AddListener(OnClickLobby);
        }
        else
        {
            Debug.LogWarning("[ClearPanel] Lobby 버튼을 찾지 못했습니다. 이름을 'ClearLobbyButton'으로 하거나 'Lobby/Home'이 포함되도록 해 주세요.");
        }
    }

    // ───────── Star: 프리팹 자동 생성 ─────────
    private void EnsureStars()
    {
        if (_starGroup == null) return;

        RefreshStars();
        if (_stars.Count >= 3) return; // 이미 있음

        GameObject prefab = Resources.Load<GameObject>(_starPrefabPath);
        if (prefab == null)
        {
            Debug.LogWarning("[ClearPanelControl] Star 프리팹을 찾지 못했습니다: Resources/" + _starPrefabPath);
            return;
        }

        int need = 3 - _stars.Count;
        for (int i = 0; i < need; i++)
        {
            GameObject go = Instantiate(prefab, _starGroup);
            go.name = "Star" + (_starGroup.childCount);
            var rt = go.transform as RectTransform;
            if (rt != null)
            {
                rt.localScale = Vector3.one;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
            }
        }
        RefreshStars();
    }

    // StarPos 아래의 StarUI 자동 수집
    private void RefreshStars()
    {
        _stars.Clear();
        if (_starGroup == null) return;

        StarUI[] found = _starGroup.GetComponentsInChildren<StarUI>(false);
        if (found == null) return;

        for (int i = 0; i < found.Length; i++) _stars.Add(found[i]);
        _stars.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
    }

    // StarPos Y 위치(중앙~상단 사이) + X 등간 배치(항상 3슬롯)
    private void RepositionStars()
    {
        if (_panelRoot == null || _starGroup == null) return;

        _starGroup.anchorMin = _starGroup.anchorMax = new Vector2(0.5f, 0.5f);
        _starGroup.pivot = new Vector2(0.5f, 0.5f);

        float parentH = _panelRoot.rect.height;
        float norm = Mathf.Clamp(_starYBetweenCenterAndTop, 0.5f, 1f);
        float yFromCenter = (norm - 0.5f) * parentH;
        _starGroup.anchoredPosition = new Vector2(0f, yFromCenter);

        int uiCount = Mathf.Min(_stars.Count, 3);
        for (int i = 0; i < uiCount; i++)
        {
            var rt = _stars[i].transform as RectTransform;
            if (rt == null) continue;

            float x = (i - (3 - 1) * 0.5f) * _starSpacing; // -spacing, 0, +spacing
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.gameObject.SetActive(true);
        }
    }

    // ───────── 공개 API ─────────
    public void Show(string stageId, NormalStageData stage, NormalStageManager.StageEndSnapshot snapshot)
    {
        SetStageId(stageId);
        ShowStars(stage, snapshot);
        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);

    public void SetStageId(string stageId)
    {
        _currentStageId = stageId;
        if (_stageNameText != null) _stageNameText.text = stageId;
    }

    public void ShowStars(NormalStageData stage, NormalStageManager.StageEndSnapshot snapshot)
    {
        EnsureStars();
        RefreshStars();

        const int SLOT_COUNT = 3;
        int met = NormalStageManager.Instance != null ?
                  NormalStageManager.Instance.EvaluateStars(stage, snapshot) : 0;
        met = Mathf.Clamp(met, 0, SLOT_COUNT);

        int useCount = Mathf.Min(_stars.Count, SLOT_COUNT);
        for (int i = 0; i < useCount; i++)
        {
            StarUI star = _stars[i];
            if (star == null) continue;
            star.gameObject.SetActive(true);
            star.InitIdle();
        }

        StopAllCoroutines();
        StartCoroutine(CoPlayStarsSequential(useCount, met));

        Canvas.ForceUpdateCanvases();
        RepositionStars();
        ApplyConditionTexts(stage);
    }

    private System.Collections.IEnumerator CoPlayStarsSequential(int useCount, int met)
    {
        if (_starSeqPlaying) yield break;
        _starSeqPlaying = true;

        for (int i = 0; i < useCount; i++)
        {
            bool success = i < met;
            if (_stars[i] != null)
                _stars[i].PlayCondition(success);

            yield return new WaitForSecondsRealtime(_starStepWait);
        }
        _starSeqPlaying = false;
    }

    // 보상: (id,value) → 아이콘은 매니저에서 조회
    public void BuildRewardsByIds(RewardItemUI rewardItemPrefab, IList<(string id, int value)> rewards)
    {
        if (_rewardsRoot == null || rewardItemPrefab == null) return;

        for (int i = _rewardsRoot.childCount - 1; i >= 0; i--)
            Destroy(_rewardsRoot.GetChild(i).gameObject);

        for (int i = 0; i < rewards.Count; i++)
        {
            Sprite icon = NormalStageManager.Instance.GetRewardIcon(rewards[i].id);
            RewardItemUI ui = Instantiate(rewardItemPrefab, _rewardsRoot);
            ui.name = "Reward_" + rewards[i].id;
            ui.Set(icon, rewards[i].value);
        }

        CenterRewards(_rewardSpacing);
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rewardsRoot);
        LayoutRebuilder.ForceRebuildLayoutImmediate(_rewardBack);
    }

    public void CenterRewards(float spacing)
    {
        if (_rewardHLG == null) return;
        _rewardHLG.childAlignment = TextAnchor.MiddleCenter;
        _rewardHLG.spacing = spacing;
        _rewardHLG.childForceExpandWidth = false;
        _rewardHLG.childForceExpandHeight = false;
    }

    public void ApplyConditionTexts(NormalStageData stage)
    {
        if (_condRoot == null) return;
        SetCondRow(_firstCondText, _firstCondIcon, GetCondInfo(stage, 0));
        SetCondRow(_secondCondText, _secondCondIcon, GetCondInfo(stage, 1));
        SetCondRow(_thirdCondText, _thirdCondIcon, GetCondInfo(stage, 2));
    }

    private static string GetCondInfo(NormalStageData stage, int index)
    {
        if (stage.Condition == null || index < 0 || index >= stage.Condition.Count) return string.Empty;
        return stage.Condition[index].Info ?? string.Empty;
    }

    private static void SetCondRow(TMP_Text text, GameObject icon, string info)
    {
        bool has = !string.IsNullOrEmpty(info);
        if (text != null)
        {
            text.gameObject.SetActive(has);
            if (has) text.text = info;
        }
        if (icon != null) icon.SetActive(has);
    }

    // ───────── 버튼 핸들러 ─────────
    private void OnClickNext()
    {
        Debug.Log("[ClearPanel] NextStage 버튼 클릭됨");
        NextStageRequested?.Invoke();
    }

    private void OnClickLobby()
    {
        LobbyRequested?.Invoke();
    }

    // 외부에서 현재 표시 중 스테이지ID를 가져가고 싶을 때
    public string GetCurrentStageId() => _currentStageId;
}
