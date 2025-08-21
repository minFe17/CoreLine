using System.Collections.Generic;
using UnityEngine;

public class RouteManager : MonoBehaviour
{
    public TestMap map;
    public PathRenderer renderer;

    [Header("Endpoints (��,��)")]
    public Vector2Int SpawnCell = new Vector2Int(0, 0);
    public Vector2Int GoalCell = new Vector2Int(5, 5);

    private List<Vector2Int> _lastPath;

    void Awake()
    {
        if (!map) map = FindAnyObjectByType<TestMap>();
    }

    void OnEnable()
    {
        if (map != null) map.OnCellChanged += HandleCellChanged;
        RebuildAndApply(force: true);
    }

    void OnDisable()
    {
        if (map != null) map.OnCellChanged -= HandleCellChanged;
    }

    void HandleCellChanged(int r, int c)
    {
        // �� �ϳ� �ٲ� ������ ��� ����, "������ �޶����� ����" ���� ������Ʈ
        RebuildAndApply(force: false);
    }

    public void RebuildAndApply(bool force)
    {
        if (map == null) { renderer?.Clear(); _lastPath = null; return; }

        var path = AStarPathfinder.FindPath(
            map.Height, map.Width,
            (r, c) => map.IsWalkable(r, c),
            SpawnCell, GoalCell
        );

        if (path == null || path.Count == 0)
        {
            renderer?.Clear();
            _lastPath = null;
            return;
        }

        if (force || IsDifferent(_lastPath, path))
        {
            renderer?.SetPath(map, path);
            _lastPath = path;

            MonsterManager.Instance?.OnRouteChanged();
        }
    }


    public void SetEndpoints(Vector2Int spawn, Vector2Int goal, bool rebuildNow = true)
    {
        SpawnCell = spawn;
        GoalCell = goal;
        if (rebuildNow) RebuildAndApply(force: true);
    }

    static bool IsDifferent(List<Vector2Int> a, List<Vector2Int> b)
    {
        if (a == null || b == null) return true;
        if (a.Count != b.Count) return true;
        for (int i = 0; i < a.Count; i++) if (a[i] != b[i]) return true;
        return false;
    }


}
