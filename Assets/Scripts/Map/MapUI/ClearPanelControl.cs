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
    private float _starYBetweenCenterAndTop = 0.58f;   // 0.5=중앙, 1.0=최상단
    private bool _autoRepositionOnResize = true;
    private float _rewardSpacing = 200f;               // 보상 간격

    private NormalStageData _lastShownStage;
    private NormalStageManager.StageEndSnapshot _lastSnapshot;
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
        Debug.Log("[ClearPanel] Awake " + name);
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

    private void AutoWire()
    {
        _panelRoot = GetComponent<RectTransform>();

        // StageName
        Transform transformStageName = transform.Find("StageName");
        if (transformStageName != null)
            _stageNameText = transformStageName.GetComponent<TMP_Text>();

        // Star group (for animated 3 stars on top)
        Transform transformStarPos = transform.Find("StarPos");
        if (transformStarPos != null)
            _starGroup = transformStarPos.GetComponent<RectTransform>();

        // Reward back + Rewards container
        Transform transformRewardBack = transform.Find("RewardBack");
        if (transformRewardBack != null)
        {
            _rewardBack = transformRewardBack.GetComponent<RectTransform>();

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

        // ─────────────────────────────────────────────
        // StarCondition (조건 텍스트/아이콘)  고정 경로 배선
        // ─────────────────────────────────────────────
        _condRoot = transform.Find("StarCondition");

        if (_condRoot != null)
        {
            // 아이콘(별) 오브젝트
            Transform firstStar = _condRoot.Find("FirstStar");
            Transform secondStar = _condRoot.Find("SecondStar");
            Transform thirdStar = _condRoot.Find("ThirdStar");

            _firstCondIcon = firstStar != null ? firstStar.gameObject : null;
            _secondCondIcon = secondStar != null ? secondStar.gameObject : null;
            _thirdCondIcon = thirdStar != null ? thirdStar.gameObject : null;

            // 텍스트(TMP)
            Transform firstText = _condRoot.Find("FirstStarText");
            Transform secondText = _condRoot.Find("SecondStarText");
            Transform thirdText = _condRoot.Find("ThirdStarText");

            _firstCondText = firstText != null ? firstText.GetComponent<TMP_Text>() : null;
            _secondCondText = secondText != null ? secondText.GetComponent<TMP_Text>() : null;
            _thirdCondText = thirdText != null ? thirdText.GetComponent<TMP_Text>() : null;
        }
    }

    private void EnsureStars()
    {
        if (_starGroup == null)
        {
            Debug.LogError("[ClearPanel] StarPos(_starGroup) 가 없습니다.");
            return;
        }

        RefreshStars();
        Debug.Log("[ClearPanel] EnsureStars before: count=" + _stars.Count);
        if (_stars.Count >= 3)
        {
            // 혹시 비활성로 나와 있으면 살려준다
            for (int i = 0; i < 3; i++)
                if (_stars[i] != null) _stars[i].gameObject.SetActive(true);
            Debug.Log("[ClearPanel] EnsureStars use existing 3");
            return;
        }

        GameObject prefab = Resources.Load<GameObject>(_starPrefabPath); // "Reward/ClearStar"
        if (prefab == null)
        {
            Debug.LogError("[ClearPanel] Star 프리팹을 찾지 못했습니다. 경로 확인: Assets/Resources/Reward/ClearStar.prefab " +
                           " 또는 StarPos 아래에 ClearStar(StarUI 포함) 3개를 직접 배치하세요.");
            return;
        }

        int need = 3 - _stars.Count;
        for (int i = 0; i < need; i++)
        {
            GameObject go = Instantiate(prefab, _starGroup);
            go.name = "Star" + (_starGroup.childCount);
            RectTransform rt = go.transform as RectTransform;
            if (rt != null)
            {
                rt.localScale = Vector3.one;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = Vector2.zero;
            }
            go.SetActive(true);
        }
        RefreshStars();
        Debug.Log("[ClearPanel] EnsureStars after: count=" + _stars.Count);
    }


    // StarPos 아래의 StarUI 자동 수집
    private void RefreshStars()
    {
        _stars.Clear();
        if (_starGroup == null) return;

        // 비활성도 포함해서 수집해야 함 (true)
        StarUI[] found = _starGroup.GetComponentsInChildren<StarUI>(true);
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
            RectTransform rt = _stars[i].transform as RectTransform;
            if (rt == null) continue;

            float x = (i - (3 - 1) * 0.5f) * _starSpacing; // -spacing, 0, +spacing
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(x, 0f);
            rt.gameObject.SetActive(true);
        }

        // 덮이지 않도록 StarPos를 가장 위로
        _starGroup.SetAsLastSibling();
    }


    // ───────── 공개 API ─────────
    public void Show(string stageId, NormalStageData stage, NormalStageManager.StageEndSnapshot snapshot)
    {
        Debug.Log("[ClearPanel] Show stageId=" + stageId + " stage.Id=" + stage.Id);
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
        Debug.Log("[ClearPanel] ShowStars START");
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

        //캐싱 추가: '다음으로' 클릭 시 안전 저장용
        _lastShownStage = stage;
        _lastSnapshot = snapshot;
        Debug.Log("[ClearPanel] ShowStars END");
    }

    private void EnsureLatestClearSaved()
    {
        NormalStageManager mgr = NormalStageManager.Instance;
        if (mgr == null) return;

        // 스테이지 ID 유효성 검사
        if (string.IsNullOrEmpty(_currentStageId)) return;
        if (string.IsNullOrEmpty(_lastShownStage.Id)) return;
        if (_lastShownStage.Id != _currentStageId) return;

        // 이번 클리어에서 획득한 별 수
        int met = mgr.EvaluateStars(_lastShownStage, _lastSnapshot);
        if (met < 0) met = 0;
        int maxPossible = (_lastShownStage.Condition != null) ? _lastShownStage.Condition.Count : 0;
        if (maxPossible > 3) maxPossible = 3;
        if (met > maxPossible) met = maxPossible;

        // 이미 저장된 최고 별 수
        ClearStage cs = mgr.GetClearStageOrNull(_currentStageId);
        int savedBest = 0;
        if (cs != null)
        {
            // MaxStarNum을 신뢰(별 플래그 합과 동일하도록 관리 중)
            savedBest = cs.MaxStarNum;
        }

        // 아직 저장이 안 되어 있으면(새 기록이면) 진행도만 저장(골드/젬 중복 방지)
        if (met > savedBest)
        {
            mgr.ApplyClearAndSave(_lastShownStage, met, false /* giveGoldEveryClear: 중복지급 방지 */);
        }
    }

    private IEnumerator CoPlayStarsSequential(int useCount, int met)
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

    private void WireConditionArea()
    {
        // 1) CondRoot 찾기: 이름에 "Cond" 또는 "Condition"이 포함된 첫 영역
        _condRoot = FindChildByNameContains(transform, "Cond", true);
        if (_condRoot == null) _condRoot = FindChildByNameContains(transform, "Condition", true);
        if (_condRoot == null)
        {
            // 못 찾으면 비활성화
            _firstCondText = _secondCondText = _thirdCondText = null;
            _firstCondIcon = _secondCondIcon = _thirdCondIcon = null;
            return;
        }

        // 2) 3개의 행(First/Second/Third) 또는 자식 인덱스로 찾아서 TMP_Text/아이콘 할당
        Transform row1 = FindChildByNameContains(_condRoot, "First", false);
        Transform row2 = FindChildByNameContains(_condRoot, "Second", false);
        Transform row3 = FindChildByNameContains(_condRoot, "Third", false);

        // 폴백: 위 키워드가 없다면 자식 인덱스 0/1/2 사용
        if (row1 == null || row2 == null || row3 == null)
        {
            if (_condRoot.childCount >= 3)
            {
                row1 ??= _condRoot.GetChild(0);
                row2 ??= _condRoot.GetChild(1);
                row3 ??= _condRoot.GetChild(2);
            }
        }

        _firstCondText = GetFirstTMPTextUnder(row1);
        _secondCondText = GetFirstTMPTextUnder(row2);
        _thirdCondText = GetFirstTMPTextUnder(row3);

        _firstCondIcon = FindIconUnder(row1);
        _secondCondIcon = FindIconUnder(row2);
        _thirdCondIcon = FindIconUnder(row3);
    }

    private static Transform FindChildByNameContains(Transform root, string keyword, bool deep)
    {
        if (root == null || string.IsNullOrEmpty(keyword)) return null;
        string k = keyword.ToLowerInvariant();

        if (root.name != null && root.name.ToLowerInvariant().IndexOf(k, StringComparison.Ordinal) >= 0)
            return root;

        int childCount = root.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform child = root.GetChild(i);
            if (child == null) continue;

            if (child.name != null && child.name.ToLowerInvariant().IndexOf(k, StringComparison.Ordinal) >= 0)
                return child;

            if (deep)
            {
                Transform hit = FindChildByNameContains(child, keyword, true);
                if (hit != null) return hit;
            }
        }
        return null;
    }

    private static TMP_Text GetFirstTMPTextUnder(Transform t)
    {
        if (t == null) return null;
        TMP_Text txt = t.GetComponentInChildren<TMP_Text>(true);
        return txt;
    }

    private static GameObject FindIconUnder(Transform t)
    {
        if (t == null) return null;

        // 이름에 "Icon" 포함된 오브젝트 우선
        Transform icon = FindChildByNameContains(t, "Icon", true);
        if (icon != null) return icon.gameObject;

        // 폴백: Image가 붙은 첫 자식 (텍스트는 제외)
        Image[] images = t.GetComponentsInChildren<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            Image img = images[i];
            if (img == null) continue;
            if (img.GetComponent<TMP_Text>() != null) continue;
            return img.gameObject;
        }
        return null;
    }

    // ───────── 버튼 핸들러 ─────────
    private void OnClickNext()
    {
        EnsureLatestClearSaved();

        Debug.Log("[ClearPanel] NextStage 버튼 클릭됨");
        Action ev = NextStageRequested;
        if (ev != null) ev.Invoke(); ;
    }

    private void OnClickLobby()
    {
        LobbyRequested?.Invoke();
    }

    // 외부에서 현재 표시 중 스테이지ID를 가져가고 싶을 때
    public string GetCurrentStageId() => _currentStageId;
}
