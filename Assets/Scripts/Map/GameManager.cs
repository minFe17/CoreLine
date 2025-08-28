using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DefaultExecutionOrder(-1000)]
public class GameManager : MonoBehaviour
{
    [Header("스테이지 프리팹 (중클릭으로 로드 테스트)")]
    [SerializeField] private GameObject stagePrefab;

    [Header("UI 프리팹")]
    [SerializeField] private BuildUI buildUIPrefab;   // 빌드 선택 링 등
    [SerializeField] private TwoButtonUI twoButtonUIPrefab; // 공용 2버튼 패널(취소/파괴 or 취소/발동)

    private Camera _cam;
    private BuildUI _buildUI;
    private TwoButtonUI _panel;

    void Awake()
    {
        // 스테이지 로드
        MapManager.Instance.LoadStage(stagePrefab);

        // 카메라/이벤트시스템/레이캐스터 보장
        _cam = Camera.main ?? FindFirstObjectByType<Camera>();
        if (FindFirstObjectByType<EventSystem>() == null)
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        if (_cam && _cam.GetComponent<Physics2DRaycaster>() == null)
            _cam.gameObject.AddComponent<Physics2DRaycaster>();

        // Canvas 확보
        var canvas = FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
        if (canvas == null)
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }

        // BuildUI 인스턴스
        _buildUI = FindFirstObjectByType<BuildUI>(FindObjectsInactive.Include);
        if (_buildUI == null && buildUIPrefab != null)
        {
            _buildUI = Instantiate(buildUIPrefab, canvas.transform);
            _buildUI.gameObject.name = "BuildUI(Runtime)";
            _buildUI.Close();
        }

        // TwoButtonUI 인스턴스
        _panel = FindFirstObjectByType<TwoButtonUI>(FindObjectsInactive.Include);
        if (_panel == null && twoButtonUIPrefab != null)
        {
            _panel = Instantiate(twoButtonUIPrefab, canvas.transform);
            _panel.gameObject.name = "TwoButtonUI(Runtime)";
            _panel.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        // 중클릭: 스테이지 리로드
        if (Input.GetMouseButtonDown(2))
        {
            if (stagePrefab)
            {
                MapManager.Instance.LoadStage(stagePrefab);
                Debug.Log("[GameManager] 스테이지 로드 완료.");
            }
            else Debug.LogError("[GameManager] stagePrefab이 비어있습니다.");
        }

        // 좌클릭
        if (Input.GetMouseButtonDown(0))
        {
            // 1) 먼저 ObjectTile 클릭인지 검사 → 맞으면 '발동' 패널 열고 종료
            if (TryOpenObjectTilePanel()) return;

            // 2) UI 위 클릭이면 무시
            if (IsPointerOverUI()) return;

            // 3) 파괴/빌드 로직
            var map = MapManager.Instance;
            if (!map || !map.IsReady) return;

            Vector3 world = _cam.ScreenToWorldPoint(Input.mousePosition);
            world.z = 0f;
            Vector3Int cell = map.WorldToCell(world);

            // 파괴 가능 벽이면 '파괴' 패널
            if (map.IsDestructible(cell))
            {
                _panel.OpenAtCell(cell, "파괴", (id, payload) =>
                {
                    map.DestroyWallAt((Vector3Int)payload);
                });
                return;
            }

            // 일반 빌드 UI
            if (_buildUI == null) return;

            var buildOptions = MakeOptionsForTest(8); // 실제 옵션으로 교체 가능
            _buildUI.OpenAtCell(cell, buildOptions, (picked, selectedCell) =>
            {
                if (picked.Prefab == null) return;
                var info = map.GetPlaceInfo(selectedCell);
                if (!info.Placeable) return;

                Vector3 pos = map.CellCenterWorld(selectedCell);
                var go = Instantiate(picked.Prefab, pos, Quaternion.identity);
                map.RegisterTower(selectedCell, go);
            });
        }
    }

    // ───────── 유틸 ─────────
    bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;
        if (EventSystem.current.IsPointerOverGameObject()) return true; // 마우스
        for (int i = 0; i < Input.touchCount; i++)
            if (EventSystem.current.IsPointerOverGameObject(Input.touches[i].fingerId))
                return true; // 터치
        return false;
    }

    // ObjectTile 클릭 시 '발동' 패널 열기
    bool TryOpenObjectTilePanel()
    {
        if (_cam == null || _panel == null) return false;

        Vector3 wp = _cam.ScreenToWorldPoint(Input.mousePosition);
        wp.z = 0f;

        // 겹침 고려해서 전 레이어 검사
        Collider2D[] hits = Physics2D.OverlapPointAll((Vector2)wp, ~0);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D h = hits[i];
            if (h && h.TryGetComponent<ObjectTile>(out ObjectTile objectTile))
            {
                _panel.OpenAtObject(objectTile, "발동", (id, payload) =>
                {
                    if (payload is ObjectTile objectTile) objectTile.Activate();
                });
                return true;
            }
        }
        return false;
    }

    // 더미/테스트 옵션 생성
    List<TowerOption> MakeOptionsForTest(int count)
    {
        var list = new List<TowerOption>(count);
        for (int i = 0; i < count; i++)
            list.Add(new TowerOption($"opt_{i + 1}", null, null, (i + 1) * 10));
        return list;
    }
}
