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
        public Vector2Int P;
        public Node Parent;
        public int G, H; 
        public int F => G + H;
        public Node(Vector2Int p, Node parent, int g, int h) { this.P = p; this.Parent = parent; this.G = g; this.H = h; }
    }

    public static List<Vector2Int> FindPath(bool[,] walkable, Vector2Int start, Vector2Int goal)
    {
        int H = walkable.GetLength(0), W = walkable.GetLength(1);
        bool InBounds(Vector2Int q) => q.x >= 0 && q.y >= 0 && q.x < H && q.y < W;
        bool IsFree(Vector2Int q) => InBounds(q) && walkable[q.x, q.y];
        int Heu(Vector2Int a, Vector2Int b) => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

        if (!IsFree(start) || !InBounds(goal)) return null;

        List<Node> open = new List<Node>();
        Dictionary<Vector2Int, Node> came = new Dictionary<Vector2Int, Node>();
        Dictionary<Vector2Int, int> bestG = new Dictionary<Vector2Int, int>();
        HashSet<Vector2Int> closed = new HashSet<Vector2Int>();

        Node startNode = new Node(start, null, 0, Heu(start, goal));
        open.Add(startNode);
        bestG[start] = 0;
        came[start] = startNode;

        while (open.Count > 0)
        {
            int bestIdx = 0;
            for (int i = 1; i < open.Count; i++) if (open[i].F < open[bestIdx].F) bestIdx = i;
            Node cur = open[bestIdx];
            open.RemoveAt(bestIdx);

            if (cur.P == goal)
            {
                var path = new List<Vector2Int>();
                var t = cur;
                while (t != null) { path.Add(t.P); t = t.Parent; }
                path.Reverse();
                return path;
            }

            closed.Add(cur.P);

            foreach (var d in DIR4)
            {
                var np = new Vector2Int(cur.P.x + d.x, cur.P.y + d.y);
                if (!IsFree(np) || closed.Contains(np)) continue;

                int ng = cur.G + 1;
                if (!bestG.TryGetValue(np, out int oldG) || ng < oldG)
                {
                    bestG[np] = ng;
                    var nn = new Node(np, cur, ng, Heu(np, goal));
                    came[np] = nn;

                    int idx = open.FindIndex(n => n.P == np);
                    if (idx >= 0) open[idx] = nn; else open.Add(nn);
                }
            }
        }

        return null; 
    }
}
