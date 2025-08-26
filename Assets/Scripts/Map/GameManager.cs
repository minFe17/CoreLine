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
            if (IsPointerOverUI()) return;  // ← 이 한 줄이 핵심!

            if (_buildUI == null) return;

            Vector3 world = _cam ? _cam.ScreenToWorldPoint(Input.mousePosition) : (Vector3)Input.mousePosition;
            world.z = 0f;

            var map = MapManager.Instance;
            if (map && map.IsReady)
                world = map.CellCenterWorld(map.WorldToCell(world));

            List<TowerOption> options = MakeOptionsForTest(8);

            _buildUI.OpenAtWorld(world, options, picked =>
            {
                Debug.Log($"[GameManager] Picked: {picked.Id}, Cost={picked.Cost}");

                if (map && map.IsReady && picked.Prefab)
                {
                    var cell = map.WorldToCell(world);
                    if (map.GetPlaceInfo(cell).Placeable)
                    {
                        var pos = map.CellCenterWorld(cell);
                        var go = Instantiate(picked.Prefab, pos, Quaternion.identity);
                        map.RegisterTower(cell, go);
                    }
                }
            });
        }
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
