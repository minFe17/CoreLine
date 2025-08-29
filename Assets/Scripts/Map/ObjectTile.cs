using UnityEngine.EventSystems;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ObjectTile : MonoBehaviour, IPointerClickHandler
{
    private Vector3Int _cell;
    private bool _registered;
    private MapManager _map;                  // ← 캐시

    private static bool s_quitting = false;   // 종료 가드

    private void Start()
    {
        _map = MapManager.Instance;           // ← 여기서만 한 번 접근 (플레이 중일 때)
        if (_map != null && _map.IsReady)
        {
            _cell = _map.WorldToCell(transform.position);
            _map.MarkOccupied(_cell);
            _registered = true;
        }
        else
        {
            Debug.LogWarning("[ObjectTile] MapManager가 아직 준비되지 않았습니다.");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {

    }

    public void Activate()
    {
        if (_registered && _map != null)
        {
            _map.UnmarkOccupied(_cell);
            _registered = false;
        }
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // 종료 중에는 아무 것도 하지 않음(새 생성 방지)
        if (s_quitting) return;

        if (_registered && _map != null)
        {
            _map.UnmarkOccupied(_cell);
            _registered = false;
        }
    }

    private void OnApplicationQuit()
    {
        s_quitting = true;
    }
}
