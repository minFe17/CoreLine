using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MonsterMover : MonoBehaviour
{
    public TestMap map;
    public float moveSpeed = 4f;
    public float arriveEps = 0.02f;

    [SerializeField] private PathRenderer pathRenderer; 
    [SerializeField] private bool trimRuntimePath = true;

    public Vector2Int Cell { get; private set; }

    Coroutine moveCo;
    Camera _cam;

    void Start()
    {
        if (!map) map = FindAnyObjectByType<TestMap>();

        _cam = Camera.main;
        Cell = map.WorldToCell(transform.position);
        transform.position = map.CellToWorld(Cell.x, Cell.y);
    }

    public void MoveToWorld(Vector3 world)
    {
        var dst = map.WorldToCell(world);
        MoveToCell(dst);
    }

    public void MoveToCell(Vector2Int dst)
    {
        var path = AStarPathfinder.FindPath(map.walkable, Cell, dst);
        if (path == null || path.Count <= 1) return;

        if (pathRenderer) pathRenderer.SetPath(path);

        if (path == null || path.Count <= 1) return;

        if (moveCo != null) StopCoroutine(moveCo);
        moveCo = StartCoroutine(Follow(path));
    }

    IEnumerator Follow(List<Vector2Int> path)
    {
        int i = (path[0] == Cell) ? 1 : 0;

        for (; i < path.Count; i++)
        {
            Vector2Int step = path[i];
            Vector3 target = map.CellToWorld(step.x, step.y);

            while ((transform.position - target).sqrMagnitude > arriveEps * arriveEps)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;
            Cell = step;

            if (trimRuntimePath && pathRenderer && path != null && i < path.Count)
            {
                var remain = path.GetRange(i, path.Count - i);
                pathRenderer.SetPath(remain);
            }
        }
        moveCo = null;
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            var wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            wp.z = 0f;
            MoveToWorld(wp);
        }
    }
}
