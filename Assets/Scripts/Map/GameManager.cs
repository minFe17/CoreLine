using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("�ν����Ϳ� �������� ������ �ϳ� ����")]
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
            // ��Ŭ��: ������ �ε�(��ε� ����)
            if (stagePrefab == null)
            {
                Debug.LogError("[GameManager_Simple] stagePrefab�� ����ֽ��ϴ�.");
                return;
            }
            MapManager.Instance.LoadStage(stagePrefab);
            Debug.Log("[GameManager_Simple] �������� �ε� �Ϸ�.");
        }

        if (Input.GetMouseButtonDown(1))
        {
            // ��Ŭ��: Ÿ�� �˻�(���콺 ��ġ ��)
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
                $"Cell {cell} �� " +
                $"Buildable:{buildable}, Unbuildable:{unbuildable}, " +
                $"Wall:{wall}, Destructible:{destructible}, Deco:{deco}, " +
                $"Occupied:{occupied} | BuildableCell? {MapManager.Instance.IsBuildableCell(cell)}";

            Debug.Log(report);
        }
    }
}
