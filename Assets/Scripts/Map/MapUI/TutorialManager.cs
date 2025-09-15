using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    // 이 튜토리얼을 보여줄 스테이지 (튜토리얼 스테이지)
    private const string TargetStageId = "Stage1-0";

    // 텍스트 사이 간격(게임 시간 기준)
    private const float IntervalSeconds = 10f;

    // 재활용 패널(이름으로 자동 배선)
    private GameObject _panelRoot;          // "Tutorial1"
    private TextMeshProUGUI _text;          // "TutorialText"
    private Button _closeBtn;               // "CloseButton"

    // 단계 텍스트 (원하는 대로 편집)
    private static readonly string[] Steps =
    {
        "몬스터가 베이스에 가는 걸 막는 타워 디펜스 장르입니다.\n" +
        "먼저 노란색 테두리 타일에 베이스를 설치해보세요.\n" +
        "베이스를 설치하고 타일을 누르면 설치 가능한 타일이 나옵니다. 타일을 눌러 유닛을 배치 해보세요.",

        "타워를 배치하면 길을 막게 됩니다. 전략적으로 배치하면 적은 유닛으로 쉽게 막을 수 있습니다.\n" +
        "몬스터가 베이스로 가는 길을 전부 막게 되면 몬스터는 타워를 공격합니다.",

        "왼쪽의 코스트는 시간이 지나면서 차오르고 배치할 때 줄어들어요.",

        "코스트는 시간이 지나면 회복돼요."
    };

    private bool _wantsClose;
    private Coroutine _flow;

    private void Awake()
    {
        // 자동 배선 (계층 이름 고정: Tutorial1 / TutorialText / CloseButton)
        var rootObj = GameObject.Find("Tutorial1");
        if (rootObj != null)
        {
            _panelRoot = rootObj;
            var t = rootObj.transform;
            _text = t.Find("TutorialText")?.GetComponent<TextMeshProUGUI>();
            _closeBtn = t.Find("CloseButton")?.GetComponent<Button>();
        }

        if (_panelRoot) _panelRoot.SetActive(false);

        if (_closeBtn)
        {
            _closeBtn.onClick.RemoveAllListeners();
            _closeBtn.onClick.AddListener(() => _wantsClose = true);
        }

        // 스테이지 로드 이벤트 구독
        EventManager.Instance.Subscribe<string>(GameManager.EVT_STAGE_LOADED, OnStageLoaded);
    }

    private void OnDestroy()
    {
        // 제네릭이 아닌 UnSubscribe( string, Delegate )을 쓰는 구현이라면 아래처럼 캐스팅해서 해제
        EventManager.Instance.UnSubscribe(GameManager.EVT_STAGE_LOADED, (System.Action<string>)OnStageLoaded);
    }

    private void OnStageLoaded(string loadedId)
    {
        // 튜토리얼 스테이지가 아닐 땐 무시
        if (!string.Equals(loadedId, TargetStageId, System.StringComparison.OrdinalIgnoreCase))
            return;

        // 이미 3★ 이상이면 노출 X
        if (IsThreeStarCleared(TargetStageId))
            return;

        // 패널/참조가 없으면 패스
        if (_panelRoot == null || _text == null || _closeBtn == null)
            return;

        // 흐름 시작
        if (_flow != null) StopCoroutine(_flow);
        _flow = StartCoroutine(CoFlow());
    }

    private IEnumerator CoFlow()
    {
        for (int i = 0; i < Steps.Length; i++)
        {
            // 1) 패널 표시 + 게임 멈춤
            _wantsClose = false;
            _text.text = Steps[i];

            _panelRoot.SetActive(true);
            Time.timeScale = 0f;              // 게임/타이머 모두 정지
            PauseControl.SetPaused(true);

            // 2) 닫기 버튼 누를 때까지 대기 (정지라서 시간 안 흐름)
            while (!_wantsClose) yield return null;

            // 3) 패널 닫고 게임 재개
            _panelRoot.SetActive(false);
            Time.timeScale = 1f;
            PauseControl.SetPaused(false);

            // 4) 다음 문구까지 '게임 시간' 10초 대기
            float passed = 0f;
            while (passed < IntervalSeconds)
            {
                passed += Time.deltaTime; // TimeScale 0이면 안 늘어남 → 자연 정지
                yield return null;
            }
        }

        _flow = null;
    }

    // GameData.ClearStage에서 해당 스테이지 MaxStarNum이 3 이상인지 확인
    private bool IsThreeStarCleared(string stageId)
    {
        var gd = DataManager.Instance?.GameData;
        if (gd?.ClearStage == null) return false;

        for (int i = 0; i < gd.ClearStage.Count; i++)
        {
            var cs = gd.ClearStage[i];
            if (cs.StageId == stageId && cs.MaxStarNum >= 3)
                return true;
        }
        return false;
    }
}
