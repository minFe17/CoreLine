using System.Collections.Generic;
using UnityEngine;

public class RouteManager : MonoBehaviour
{
    public TestMap Map;
    public Vector2Int SpawnCell;
    public Vector2Int GoalCell;

    public PathRenderer Renderer;

    public bool AutoDetectMapChange = true;

    private List<Vector2Int> _lastPath;
    private int _lastWalkableHash;

    void Start()
    {
        if (Renderer) Renderer.SetMap(Map);
        RebuildAndApply(force: true);
        if (AutoDetectMapChange)
            _lastWalkableHash = HashWalkable(Map);
    }

    void Update()
    {
        if (!AutoDetectMapChange) return;

        int h = HashWalkable(Map);
        if (h != _lastWalkableHash)
        {
            _lastWalkableHash = h;
            RebuildAndApply(force: false);
        }
    }

    public void RebuildAndApply(bool force)
    {
        var newPath = AStarPathfinder.FindPath(Map.Walkable, SpawnCell, GoalCell);
       
        if (newPath == null || newPath.Count == 0)
        {
            Renderer?.Clear();
            _lastPath = null;
            return;
        }

        if (force || IsDifferent(_lastPath, newPath))
        {
            Renderer?.SetPath(newPath);
            _lastPath = newPath;

            if (!force) MonsterManager.Instance?.OnRouteChanged();
        }
        
    }

    public void SetEndpoints(Vector2Int spawn, Vector2Int goal, bool rebuildNow = true)
    {
        SpawnCell = spawn;
        GoalCell = goal;
        if (rebuildNow) RebuildAndApply(force: true);
    }

    private static bool IsDifferent(List<Vector2Int> a, List<Vector2Int> b)
    {
        if (a == null || b == null) return true;
        if (a.Count != b.Count) return true;
        for (int i = 0; i < a.Count; i++)
            if (a[i] != b[i]) return true;
        return false;
    }

    private static int HashWalkable(TestMap map)
    {
        if (map == null || map.Walkable == null) return 0;
        int H = map.Walkable.GetLength(0);
        int W = map.Walkable.GetLength(1);
        unchecked
        {
            int h = 17;
            for (int r = 0; r < H; r++)
                for (int c = 0; c < W; c++)
                    h = h * 31 + (map.Walkable[r, c] ? 1 : 0);
            return h;
        }
    }
}
