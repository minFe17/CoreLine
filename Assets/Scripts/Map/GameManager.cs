using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("인스펙터에 스테이지 프리팹 하나 연결")]
    [SerializeField] private GameObject stagePrefab;

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
        if (_cam == null) _cam = FindFirstObjectByType<Camera>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 좌클릭: 프리팹 로드(재로드 포함)
            if (stagePrefab == null)
            {
                Debug.LogError("[GameManager_Simple] stagePrefab이 비어있습니다.");
                return;
            }
            MapManager.Instance.LoadStage(stagePrefab);
            Debug.Log("[GameManager_Simple] 스테이지 로드 완료.");
        }

        if (Input.GetMouseButtonDown(1))
        {
            // 우클릭: 타일 검사(마우스 위치 셀)
            if (!MapManager.Instance.IsReady || _cam == null) return;

            Vector3 mouseWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            var cell = MapManager.Instance.WorldToCell(mouseWorld);

            MapManager.Instance.GetCellFlags(
                cell,
                out bool buildable, out bool unbuildable,
                out bool wall, out bool destructible,
                out bool deco, out bool occupied
            );

            string report =
                $"Cell {cell} → " +
                $"Buildable:{buildable}, Unbuildable:{unbuildable}, " +
                $"Wall:{wall}, Destructible:{destructible}, Deco:{deco}, " +
                $"Occupied:{occupied} | BuildableCell? {MapManager.Instance.IsBuildableCell(cell)}";

            Debug.Log(report);
        }
    }
}
