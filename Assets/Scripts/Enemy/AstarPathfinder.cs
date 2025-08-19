using System.Collections.Generic;
using UnityEngine;

public static class AStarPathfinder
{
    static readonly Vector2Int[] DIR4 = {
        new Vector2Int( 1, 0), new Vector2Int(-1, 0),
        new Vector2Int( 0, 1), new Vector2Int( 0,-1)
    };

    class Node
    {
        public Vector2Int p;
        public Node parent;
        public int g, h; 
        public int f => g + h;
        public Node(Vector2Int p, Node parent, int g, int h) { this.p = p; this.parent = parent; this.g = g; this.h = h; }
    }

    public static List<Vector2Int> FindPath(bool[,] walkable, Vector2Int start, Vector2Int goal)
    {
        int H = walkable.GetLength(0), W = walkable.GetLength(1);
        bool InBounds(Vector2Int q) => q.x >= 0 && q.y >= 0 && q.x < H && q.y < W;
        bool IsFree(Vector2Int q) => InBounds(q) && walkable[q.x, q.y];
        int Heu(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        if (!IsFree(start) || !InBounds(goal)) return null;

        var open = new List<Node>();
        var came = new Dictionary<Vector2Int, Node>();
        var bestG = new Dictionary<Vector2Int, int>();
        var closed = new HashSet<Vector2Int>();

        Node startNode = new Node(start, null, 0, Heu(start, goal));
        open.Add(startNode);
        bestG[start] = 0;
        came[start] = startNode;

        while (open.Count > 0)
        {
            int bestIdx = 0;
            for (int i = 1; i < open.Count; i++) if (open[i].f < open[bestIdx].f) bestIdx = i;
            Node cur = open[bestIdx];
            open.RemoveAt(bestIdx);

            if (cur.p == goal)
            {
                var path = new List<Vector2Int>();
                var t = cur;
                while (t != null) { path.Add(t.p); t = t.parent; }
                path.Reverse();
                return path;
            }

            closed.Add(cur.p);

            foreach (var d in DIR4)
            {
                var np = new Vector2Int(cur.p.x + d.x, cur.p.y + d.y);
                if (!IsFree(np) || closed.Contains(np)) continue;

                int ng = cur.g + 1;
                if (!bestG.TryGetValue(np, out int oldG) || ng < oldG)
                {
                    bestG[np] = ng;
                    var nn = new Node(np, cur, ng, Heu(np, goal));
                    came[np] = nn;

                    int idx = open.FindIndex(n => n.p == np);
                    if (idx >= 0) open[idx] = nn; else open.Add(nn);
                }
            }
        }

        return null; 
    }
}
