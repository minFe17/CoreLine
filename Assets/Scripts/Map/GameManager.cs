using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("스테이지 프리팹 (중클릭으로 로드 테스트)")]
    [SerializeField] private GameObject stagePrefab;

    [Header("BuildUI 프리팹 (없으면 Resources/UI/BuildUI 에서 시도)")]
    [SerializeField] private BuildUI buildUIPrefab;
    [SerializeField] private Sprite _cancelIcon;
    [SerializeField] private Sprite _destroyIcon;
    private Camera _cam;
    private BuildUI _buildUI; // 런타임 인스턴스(또는 씬 상주 인스턴스)

    void Awake()
    {
        // 1) 카메라 확보
        _cam = Camera.main;
        if (_cam == null) _cam = FindFirstObjectByType<Camera>();

        // 2) 씬에 이미 BuildUI가 있으면 그걸 사용(비활성 포함 탐색)
        _buildUI = FindFirstObjectByType<BuildUI>(FindObjectsInactive.Include);

        // 3) 없으면 프리팹/리소스에서 생성
        if (_buildUI == null)
        {
            // 캔버스 확보(없으면 생성)
            var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas == null)
            {
                var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = go.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            // EventSystem 확보(없으면 생성)
            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            // 프리팹 원본 확보(SerializeField > Resources)
            var prefab = buildUIPrefab;
            if (prefab == null)
                prefab = Resources.Load<BuildUI>("UI/BuildUI");

            if (prefab != null)
            {
                _buildUI = Instantiate(prefab, canvas.transform);
                _buildUI.gameObject.name = "BuildUI(Runtime)";
                _buildUI.Close(); // 시작 시 닫아두기
            }
            else
            {
                Debug.LogWarning("[GameManager] BuildUI 프리팹이 없습니다. " +
                                 "Inspector의 buildUIPrefab에 드래그하거나 Resources/UI/BuildUI 경로에 두세요.");
            }
        }
    }

    void Update()
    {
        // 중클릭: 스테이지 로드(테스트)
        if (Input.GetMouseButtonDown(2))
        {
            if (stagePrefab == null)
            {
                Debug.LogError("[GameManager] stagePrefab이 비어있습니다.");
            }
            else
            {
                MapManager.Instance.LoadStage(stagePrefab);
                Debug.Log("[GameManager] 스테이지 로드 완료.");
            }
        }

        // 좌클릭: 현재 위치에 빌드 UI 띄우기
        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI()) return;
            if (_buildUI == null) return;

            var cam = Camera.main;
            Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0f;

            var map = MapManager.Instance;
            Vector3Int cell = default;
            if (map && map.IsReady)
            {
                cell = map.WorldToCell(world);
                world = map.CellCenterWorld(cell); // 스냅
            }

            // 1) 파괴 가능 벽 확인 모드
            if (map && map.IsReady && map.IsDestructible(cell))
            {
                _buildUI.UseLayoutOnce(1, 1);

                var options = new List<TowerOption>
        {
            new TowerOption("cancel",  _cancelIcon,  null, 0), // 왼쪽
            new TowerOption("destroy", _destroyIcon, null, 0), // 오른쪽
        };

                _buildUI.OpenAtCell(cell, options, (picked, selectedCell) =>
                {
                    if (picked.Id == "destroy")
                        map.DestroyWallAt(selectedCell);
                    // cancel은 아무 동작 X (Close는 BuildUI가 처리)
                });
                return;
            }

            // 2) 일반 배치 모드
            var buildOptions = MakeOptionsForTest(8); // or 실제 옵션
            _buildUI.OpenAtCell(cell, buildOptions, (picked, selectedCell) =>
            {
                if (map && map.IsReady && picked.Prefab != null)
                {
                    var info = map.GetPlaceInfo(selectedCell);
                    if (!info.Placeable) return;

                    Vector3 pos = map.CellCenterWorld(selectedCell);   //셀 정중앙
                    var go = Instantiate(picked.Prefab, pos, Quaternion.identity);
                    map.RegisterTower(selectedCell, go);
                }
            });
        }
        bool IsPointerOverUI()
        {
            if (EventSystem.current == null) return false;

            // 마우스
            if (EventSystem.current.IsPointerOverGameObject())
                return true;

            // 터치(모바일 고려)
            for (int i = 0; i < Input.touchCount; i++)
                if (EventSystem.current.IsPointerOverGameObject(Input.touches[i].fingerId))
                    return true;

            return false;
        }
        // ─────────────────────────────────────────────────────────────
        // 더미/테스트 옵션 생성: 아이콘/프리팹 없이도 UI가 뜨도록
        // ─────────────────────────────────────────────────────────────
        List<TowerOption> MakeOptionsForTest(int count)
        {
            var list = new List<TowerOption>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(new TowerOption(
                    id: $"opt_{i + 1}",
                    icon: null,       // 필요하면 Sprite 연결
                    prefab: null,     // 필요하면 실제 타워 프리팹 연결
                    cost: (i + 1) * 10
                ));
            }
            return list;
        }
    }
}
