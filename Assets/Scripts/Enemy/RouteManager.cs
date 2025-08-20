using System.Collections.Generic;
using UnityEngine;

public class RouteManager : MonoBehaviour
{
    public TestMap map;
    public Vector2Int spawnCell;
    public Vector2Int goalCell;

    public PathRenderer renderer;

    public bool autoDetectMapChange = true;

    private List<Vector2Int> _lastPath;
    private int _lastWalkableHash;

    void Start()
    {
        if (renderer) renderer.SetMap(map);
        RebuildAndApply(force: true);
        if (autoDetectMapChange)
            _lastWalkableHash = HashWalkable(map);
    }

    void Update()
    {
        if (!autoDetectMapChange) return;

        int h = HashWalkable(map);
        if (h != _lastWalkableHash)
        {
            _lastWalkableHash = h;
            RebuildAndApply(force: false);
        }
    }

    public void RebuildAndApply(bool force)
    {
        var newPath = AStarPathfinder.FindPath(map.walkable, spawnCell, goalCell);
       
        if (newPath == null || newPath.Count == 0)
        {
            renderer?.Clear();
            _lastPath = null;
            return;
        }

        if (force || IsDifferent(_lastPath, newPath))
        {
            renderer?.SetPath(newPath);
            _lastPath = newPath;

            if (!force) MonsterManager.Instance?.OnRouteChanged();
        }
        
    }

    public void SetEndpoints(Vector2Int spawn, Vector2Int goal, bool rebuildNow = true)
    {
        spawnCell = spawn;
        goalCell = goal;
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
        if (map == null || map.walkable == null) return 0;
        int H = map.walkable.GetLength(0);
        int W = map.walkable.GetLength(1);
        unchecked
        {
            int h = 17;
            for (int r = 0; r < H; r++)
                for (int c = 0; c < W; c++)
                    h = h * 31 + (map.walkable[r, c] ? 1 : 0);
            return h;
        }
    }
}
