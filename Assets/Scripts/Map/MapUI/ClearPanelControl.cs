using System;
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

    private System.Collections.IEnumerator CoRepositionAfterLayout()
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

    private void AutoWire()
    {
        _panelRoot = GetComponent<RectTransform>();

        Transform tStageName = transform.Find("StageName");
        if (tStageName != null) _stageNameText = tStageName.GetComponent<TMP_Text>();

        Transform tStarPos = transform.Find("StarPos");
        if (tStarPos != null) _starGroup = tStarPos.GetComponent<RectTransform>();

        Transform tRewardBack = transform.Find("RewardBack");
        if (tRewardBack != null)
        {
            _rewardBack = tRewardBack.GetComponent<RectTransform>();

            // Rewards 컨테이너 확보(없으면 생성)
            Transform tRewards = transform.Find("RewardBack/Rewards");
            if (tRewards == null)
            {
                GameObject go = new GameObject("Rewards", typeof(RectTransform));
                tRewards = go.transform;
                tRewards.SetParent(_rewardBack, false);
            }
            _rewardsRoot = tRewards as RectTransform;
            _rewardsRoot.anchorMin = _rewardsRoot.anchorMax = new Vector2(0.5f, 0.5f);
            _rewardsRoot.pivot = new Vector2(0.5f, 0.5f);
            _rewardsRoot.anchoredPosition = Vector2.zero;

            // RewardBack에 붙어 있던 레이아웃 제거(늘어남/줄어듦 방지)
            DestroyImmediate(_rewardBack.GetComponent<HorizontalLayoutGroup>());
            DestroyImmediate(_rewardBack.GetComponent<ContentSizeFitter>());

            // Rewards에만 레이아웃 부착
            _rewardHLG = _rewardsRoot.GetComponent<HorizontalLayoutGroup>();
            if (_rewardHLG == null) _rewardHLG = _rewardsRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            _rewardHLG.childAlignment = TextAnchor.MiddleCenter;
            _rewardHLG.spacing = _rewardSpacing;
            _rewardHLG.childControlWidth = true;
            _rewardHLG.childControlHeight = true;
            _rewardHLG.childForceExpandWidth = false;
            _rewardHLG.childForceExpandHeight = false;

            _rewardCSF = _rewardsRoot.GetComponent<ContentSizeFitter>();
            if (_rewardCSF == null) _rewardCSF = _rewardsRoot.gameObject.AddComponent<ContentSizeFitter>();
            _rewardCSF.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            _rewardCSF.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            // 데코는 레이아웃 제외
            Transform tOutline = transform.Find("RewardBack/RewardOutLine");
            if (tOutline != null)
            {
                var le = tOutline.GetComponent<LayoutElement>() ?? tOutline.gameObject.AddComponent<LayoutElement>();
                le.ignoreLayout = true;
                tOutline.SetSiblingIndex(0);
            }
        }

        // StarCondition
        Transform tCond = transform.Find("StarCondition");
        if (tCond != null)
        {
            _condRoot = tCond;

            Transform t = tCond.Find("FirstStarText"); if (t) _firstCondText = t.GetComponent<TMP_Text>();
            t = tCond.Find("SecondStarText"); if (t) _secondCondText = t.GetComponent<TMP_Text>();
            t = tCond.Find("ThirdStarText"); if (t) _thirdCondText = t.GetComponent<TMP_Text>();

            t = tCond.Find("FirstStar"); if (t) _firstCondIcon = t.gameObject;
            t = tCond.Find("SecondStar"); if (t) _secondCondIcon = t.gameObject;
            t = tCond.Find("ThirdStar"); if (t) _thirdCondIcon = t.gameObject;
        }

        // Buttons
        var tNext = transform.Find("NextStageButton");
        var tLobby = transform.Find("ClearLobbyButton");
        if (tNext) _nextButton = tNext.GetComponent<Button>();
        if (tLobby) _lobbyButton = tLobby.GetComponent<Button>();

        if (_nextButton != null)
        {
            _nextButton.onClick.RemoveAllListeners();
            _nextButton.onClick.AddListener(OnClickNext);
        }
        if (_lobbyButton != null)
        {
            _lobbyButton.onClick.RemoveAllListeners();
            _lobbyButton.onClick.AddListener(OnClickLobby);
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
        NextStageRequested?.Invoke();
    }

    private void OnClickLobby()
    {
        LobbyRequested?.Invoke();
    }

    // 외부에서 현재 표시 중 스테이지ID를 가져가고 싶을 때
    public string GetCurrentStageId() => _currentStageId;
}
