using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MonsterMover : MonoBehaviour
{
    public TestMap map;
    public float moveSpeed = 4f;
    public float arriveEps = 0.02f;


    public bool IsFollowingPath { get; private set; }
    private Monster _monster;

    public Vector2Int Cell { get; private set; }

    Coroutine moveCo;

    void Start()
    {
        if (!map) map = FindAnyObjectByType<TestMap>();
        _monster = GetComponent<Monster>();

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
        List<Vector2Int> path = AStarPathfinder.FindPath(map.walkable, Cell, dst);
        if (path == null || path.Count <= 1) { 
            IsFollowingPath = false;
            return; 
        }

        if (moveCo != null) 
            StopCoroutine(moveCo);

        IsFollowingPath = true;
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
                if (_monster != null)
                {
                    Vector3 delta = target - transform.position;
                    _monster.SetFlip(delta);
                }
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;
            Cell = step;
        }
        IsFollowingPath = false;
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
