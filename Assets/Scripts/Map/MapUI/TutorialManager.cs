using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using Utils; // ConditionControl, SimpleSingleton 등 쓰는 프로젝트라면 필요
using System.Reflection;

public class TutorialManager : MonoBehaviour
{
    private const string TutorialStageId = "Stage1-0";
    private const float IntervalSeconds = 10f;

    // 재활용 패널(계층 이름 고정: Tutorial1 / TutorialText / CloseButton)
    private GameObject _panelRoot;
    private TextMeshProUGUI _text;
    private Button _closeBtn;

    private bool _wantsClose;
    private Coroutine _flow;
    private bool _handlingDefeat;

    // 안내 문구 (원하는대로 수정 가능)
    private static readonly string[] Steps = {
        "몬스터가 베이스에 가는 걸 막는 타워 디펜스 장르입니다.\n" +
        "먼저 노란색 테두리 타일에 베이스를 설치해보세요.\n" +
        "베이스를 설치하고 타일을 누르면 설치 가능한 타일이 나옵니다. 타일을 눌러 유닛을 배치 해보세요.",
        "타워를 배치하면 길을 막게 됩니다. 전략적으로 배치하면 적은 유닛으로 쉽게 막을 수 있습니다.\n" +
        "몬스터가 베이스로 가는 길을 전부 막게 되면 몬스터는 타워를 공격합니다.",
        "왼쪽의 코스트는 시간이 지나면 차오르고, 유닛을 배치하면 줄어듭니다.",
        "코스트는 시간이 지나면 다시 회복됩니다.",
        "유닛을 클릭해 강화하세요.\n" +"최대 레벨이 되면 합성 할 수 있습니다.",
    };

    private void Awake()
    {
        // 패널 오토 배선
        var rootObj = GameObject.Find("Tutorial1");
        if (rootObj != null)
        {
            _panelRoot = rootObj;
            var t = rootObj.transform;
            _text = t.Find("TutorialText")?.GetComponent<TextMeshProUGUI>();
            _closeBtn = t.Find("CloseButton")?.GetComponent<Button>();
        }
        if (_panelRoot) _panelRoot.SetActive(false);
        if (_closeBtn != null)
        {
            _closeBtn.onClick.RemoveAllListeners();
            _closeBtn.onClick.AddListener(() => _wantsClose = true);
        }

        // 스테이지 로드 이벤트 구독
        EventManager.Instance.Subscribe<string>(GameManager.EVT_STAGE_LOADED, OnStageLoaded);
    }

    private void OnDestroy()
    {
        // 구현에 따라 UnSubscribe 시그니처가 다를 수 있으니 프로젝트 EventManager에 맞게 유지
        EventManager.Instance.UnSubscribe(GameManager.EVT_STAGE_LOADED, (System.Action<string>)OnStageLoaded);
    }

    private void OnStageLoaded(string loadedId)
    {
        if (!string.Equals(loadedId, TutorialStageId, System.StringComparison.OrdinalIgnoreCase))
            return;

        // 이미 3★면 튜토리얼 스킵
        if (IsThreeStarCleared(TutorialStageId)) return;

        // 타이머 500초로 세팅(진행은 멈춘 상태로 리셋)
        var timer = FindFirstObjectByType<TimerPanelUI>(FindObjectsInactive.Include);
        if (timer != null) timer.SetDuration(500f, restartToFull: true);

        // 안내 플로우 시작
        if (_panelRoot == null || _text == null || _closeBtn == null) return;
        if (_flow != null) StopCoroutine(_flow);
        _flow = StartCoroutine(CoFlow());
    }

    private IEnumerator CoFlow()
    {
        for (int i = 0; i < Steps.Length; i++)
        {
            ShowPanel(Steps[i]);
            yield return WaitButtonThenResume();

            // 다음 문구까지 '게임 시간' 10초 대기 (패널 닫힌 뒤에만 흐름)
            float t = 0f;
            while (t < IntervalSeconds)
            {
                t += Time.deltaTime; // TimeScale=1에서만 진행
                yield return null;
            }
        }

        _flow = null;
    }

    // ───────── 패배를 가로채서 '설명 패널 → 승리 전환' 처리 ─────────
    public bool TryHandleDefeatAndConvertToWin(NormalStageData stage)
    {
        if (!string.Equals(stage.Id, TutorialStageId, System.StringComparison.OrdinalIgnoreCase))
            return false;
        if (IsThreeStarCleared(TutorialStageId)) return false; // 이미 끝낸 유저면 가로채지 않음
        if (_handlingDefeat) return true; // 이미 처리중이면 GameManager는 그냥 리턴

        _handlingDefeat = true;
        StartCoroutine(CoHandleDefeatThenWin(stage));
        return true;
    }

    private IEnumerator CoHandleDefeatThenWin(NormalStageData stage)
    {
        ShowPanel("기지가 파괴되면 패배입니다.\n이번 튜토리얼은 여기까지입니다.");
        yield return WaitButtonThenResume();

        ForceSaveThreeStars("Stage1-0");

        NormalStageManager normalStageManager = NormalStageManager.Instance;
        if (normalStageManager != null)
        {
            NormalStageManager.StageEndSnapshot snapshot = ConditionControl.BuildFor(stage);
            normalStageManager.CompleteStageSuccess(snapshot); // 승리 패널 표시
        }

        _handlingDefeat = false;
    }
    // TutorialManager.cs
    // 튜토리얼 3별 강제 저장(필드/프로퍼티 모두 지원)
    private void ForceSaveThreeStars(string stageId)
    {
        DataManager dataManager = DataManager.Instance;
        if (dataManager == null || dataManager.GameData == null || string.IsNullOrEmpty(stageId)) return;

        object gameData = dataManager.GameData;

        object clearStageListObject = GetMemberValue(gameData, "ClearStage");
        System.Collections.IList list = clearStageListObject as System.Collections.IList;
        if (list == null) return;

        bool updated = false;

        for (int i = 0; i < list.Count; i++)
        {
            object item = list[i];
            if (item == null) continue;

            object idObject = GetMemberValue(item, "StageId");
            string idString = idObject as string;
            if (!string.IsNullOrEmpty(idString) &&
                string.Equals(idString, stageId, System.StringComparison.OrdinalIgnoreCase))
            {
                SetMemberValue(item, "MaxStarNum", 3);

                object starObject = GetMemberValue(item, "Star");
                if (starObject == null)
                {
                    System.Type starType = GetMemberType(item, "Star");
                    if (starType != null)
                    {
                        object newStar = System.Activator.CreateInstance(starType);
                        SetMemberValue(newStar, "FirstStar", true);
                        SetMemberValue(newStar, "SecondStar", true);
                        SetMemberValue(newStar, "ThirdStar", true);
                        SetMemberValue(item, "Star", newStar);
                    }
                }
                else
                {
                    SetMemberValue(starObject, "FirstStar", true);
                    SetMemberValue(starObject, "SecondStar", true);
                    SetMemberValue(starObject, "ThirdStar", true);
                }

                updated = true;
                break;
            }
        }

        if (!updated)
        {
            System.Type itemType = list.GetType().GetGenericArguments()[0];
            object added = System.Activator.CreateInstance(itemType);
            SetMemberValue(added, "StageId", stageId);
            SetMemberValue(added, "MaxStarNum", 3);

            System.Type starTypeForNew = GetMemberType(added, "Star");
            if (starTypeForNew != null)
            {
                object newStar = System.Activator.CreateInstance(starTypeForNew);
                SetMemberValue(newStar, "FirstStar", true);
                SetMemberValue(newStar, "SecondStar", true);
                SetMemberValue(newStar, "ThirdStar", true);
                SetMemberValue(added, "Star", newStar);
            }

            list.Add(added);
        }

        // 저장 루틴이 있으면 호출(없으면 조용히 무시)
        try
        {
            System.Reflection.MethodInfo mi = dataManager.GetType().GetMethod("SaveData", System.Type.EmptyTypes);
            if (mi != null) mi.Invoke(dataManager, null);
        }
        catch { }

        // ★ 세션 보장 플래그: 이번 실행부터는 무조건 1-1로 가게
        PlayerPrefs.SetInt("Tutorial_Cleared3Star", 1);
        PlayerPrefs.Save();

        Debug.Log("[Tutorial] Stage1-0을 3별로 기록했습니다. (세션 플래그 on)");
    }


    // 보조: 멤버 타입 얻기(필드/프로퍼티)
    private Type GetMemberType(object instance, string name)
    {
        if (instance == null) return null;
        Type type = instance.GetType();

        PropertyInfo prop = type.GetProperty(name);
        if (prop != null) return prop.PropertyType;

        FieldInfo field = type.GetField(name);
        if (field != null) return field.FieldType;

        return null;
    }

    private void TrySaveData(DataManager dataManager)
    {
        try
        {
            MethodInfo mi = dataManager.GetType().GetMethod("SaveData", Type.EmptyTypes);
            if (mi != null) mi.Invoke(dataManager, null);
        }
        catch
        {
            // 로컬/클라우드 저장이 비활성일 수 있음 → 무시
        }
    }

    // GameManager.cs 쪽 유틸과 동일한 버전이 없다면 여기도 포함
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



    // ───────── 공통 UI 제어 ─────────
    private void ShowPanel(string msg)
    {
        _wantsClose = false;
        if (_text) _text.text = msg;
        if (_panelRoot) _panelRoot.SetActive(true);

        Time.timeScale = 0f;
        PauseControl.SetPaused(true);
    }

    private IEnumerator WaitButtonThenResume()
    {
        while (!_wantsClose) yield return null;

        if (_panelRoot) _panelRoot.SetActive(false);
        Time.timeScale = 1f;
        PauseControl.SetPaused(false);
    }

    // ───────── 3★ 클리어 여부 판단 ─────────
    private bool IsThreeStarCleared(string stageId)
    {
        var gd = DataManager.Instance?.GameData;
        if (gd?.ClearStage == null) return false;

        foreach (var cs in gd.ClearStage)
            if (cs.StageId == stageId && cs.MaxStarNum >= 3)
                return true;
        return false;
    }
}
