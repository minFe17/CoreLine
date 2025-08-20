using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MonsterMover : MonoBehaviour
{
    public TestMap map;
    public float moveSpeed = 4f;
    public float arriveEps = 0.02f;



    public Vector2Int Cell { get; private set; }

    private Vector2Int _dstCell;
    private bool _hasDestination;
    public bool IsFollowingPath { get; private set; }  

    private Monster _monster;
    private Coroutine moveCo;

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
        _dstCell = dst;
        _hasDestination = true;

        var path = AStarPathfinder.FindPath(map.walkable, Cell, dst);
        if (path == null || path.Count <= 1)
        {
            IsFollowingPath = false;
            return;
        }

        
        if (moveCo != null) StopCoroutine(moveCo);
        IsFollowingPath = true;
        moveCo = StartCoroutine(Follow(path));
    }

    IEnumerator Follow(List<Vector2Int> path)
    {
        int i = (path[0] == Cell) ? 1 : 0;

        for (; i < path.Count; i++)
        {
            Vector2Int step = path[i];
            if (_hasDestination && !map.IsWalkable(step.x, step.y))
            {
                var newPath = AStarPathfinder.FindPath(map.walkable, Cell, _dstCell);
                if (newPath != null && newPath.Count > 1)
                {
                    path = newPath;
                    i = (path[0] == Cell) ? 1 : 0;
                    step = path[i];
                }
                else
                {
                    
                    IsFollowingPath = false;
                    moveCo = null;
                    yield break;
                }
            }

            Vector3 target = map.CellToWorld(step.x, step.y);

            // 목표 셀로 이동
            while ((transform.position - target).sqrMagnitude > arriveEps * arriveEps)
            {
                if (_hasDestination && !map.IsWalkable(step.x, step.y))
                    break;
                Vector3 prev = transform.position;
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

                // 진행 방향으로 스프라이트 좌우 반전
                if (_monster != null)
                {
                    Vector3 delta = transform.position - prev;
                    _monster.SetFlip(delta);
                }

                yield return null;
            }

            if ((transform.position - target).sqrMagnitude > arriveEps * arriveEps)
            {
                i--;   
                continue;
            }

            transform.position = target;
            Cell = step;
        }

        IsFollowingPath = false;
        moveCo = null;
    }

    void Update()
    {
        // 테스트용 우클릭 이동(원한다면 유지)
        if (Input.GetMouseButtonDown(1))
        {
            var wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            wp.z = 0f;
            MoveToWorld(wp);
        }
    }
}
