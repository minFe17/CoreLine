using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MonsterMover : MonoBehaviour
{
    public TestMap Map;
    public float moveSpeed = 4f;
    public float arriveEps = 0.02f;

    public Vector2Int Cell { get; private set; } // (r,c)
    public bool IsFollowingPath { get; private set; }

    Vector2Int _dstCell;
    bool _hasDestination;
    Coroutine _moveCo;
    Monster _monster;

    void Start()
    {
        if (!Map) Map = FindAnyObjectByType<TestMap>();
        _monster = GetComponent<Monster>();

        var rc = Map.WorldToCell(transform.position);
        Cell = rc;
        transform.position = Map.CellToWorld(rc.x, rc.y);
    }

    public void MoveToWorld(Vector3 world)
    {
        var dst = Map.WorldToCell(world);
        MoveToCell(dst);
    }

    public void MoveToCell(Vector2Int dst)
    {
        _dstCell = dst;
        _hasDestination = true;

        var path = AStarPathfinder.FindPath(
            Map.Height, Map.Width,
            (r, c) => Map.IsWalkable(r, c),
            Cell, _dstCell
        );

        if (path == null || path.Count <= 1)
        {
            IsFollowingPath = false;
            _hasDestination = false;
            if (_moveCo != null) { StopCoroutine(_moveCo); _moveCo = null; }
            return;
        }

        if (_moveCo != null) StopCoroutine(_moveCo);
        IsFollowingPath = true;
        _moveCo = StartCoroutine(Follow(path));
    }

    IEnumerator Follow(List<Vector2Int> path)
    {
        int i = (path[0] == Cell) ? 1 : 0;

        for (; i < path.Count; i++)
        {
            Vector2Int step = path[i];

            // 다음 스텝이 막혔으면 재탐색
            if (_hasDestination && !Map.IsWalkable(step.x, step.y))
            {
                var newPath = AStarPathfinder.FindPath(
                    Map.Height, Map.Width,
                    (r, c) => Map.IsWalkable(r, c),
                    Cell, _dstCell
                );

                if (newPath != null && newPath.Count > 1)
                {
                    path = newPath;
                    i = (path[0] == Cell) ? 1 : 0;
                    step = path[i];
                }
                else
                {
                    IsFollowingPath = false;
                    _hasDestination = false;
                    _moveCo = null;
                    yield break;
                }
            }

            Vector3 target = Map.CellToWorld(step.x, step.y);
            while ((transform.position - target).sqrMagnitude > arriveEps * arriveEps)
            {
                // 가는 도중 막히면 루프 탈출해 다음 프레임에 재탐색
                if (_hasDestination && !Map.IsWalkable(step.x, step.y))
                    break;

                Vector3 prev = transform.position;
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

                if (_monster != null)
                {
                    Vector3 delta = transform.position - prev;
                    _monster.SetFlip(delta);
                }
                yield return null;
            }

            if ((transform.position - target).sqrMagnitude > arriveEps * arriveEps)
            {
                i--; // 같은 인덱스 재도전(재탐색 기회)
                continue;
            }

            transform.position = target;
            Cell = step;
        }

        IsFollowingPath = false;
        _moveCo = null;
    }

    void Update()
    {
        // 테스트: 우클릭으로 목적지 지정
        if (Input.GetMouseButtonDown(1))
        {
            var wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            wp.z = 0f;
            MoveToWorld(wp);
        }
    }
}
