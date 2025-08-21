using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MonsterMover : MonoBehaviour
{
    [SerializeField] private TestMap _map;
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _arriveEps = 0.02f;

    public TestMap Map {
        get { return _map; }
        set { _map = value; }
    }

    public Vector2Int Cell { get; private set; } 
    public bool IsFollowingPath { get; private set; }

    private Vector2Int _dstCell;
    private bool _hasDestination;
    private Coroutine _moveCo;
    private Monster _monster;

    private void Start()
    {
        if (!_map) _map = FindAnyObjectByType<TestMap>();
        _monster = GetComponent<Monster>();

        Vector2Int rc = _map.WorldToCell(transform.position);
        Cell = rc;
        transform.position = _map.CellToWorld(rc.x, rc.y);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            wp.z = 0f;
            MoveToWorld(wp);
        }
    }

    public void MoveToWorld(Vector3 world)
    {
        Vector2Int dst = _map.WorldToCell(world);
        MoveToCell(dst);
    }

    public void MoveToCell(Vector2Int dst)
    {
        _dstCell = dst;
        _hasDestination = true;

        List<Vector2Int> path = AStarPathfinder.FindPath(
            _map.Height, _map.Width,
            (r, c) => _map.IsWalkable(r, c),
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

    private IEnumerator Follow(List<Vector2Int> path)
    {
        int i = (path[0] == Cell) ? 1 : 0;

        for (; i < path.Count; i++)
        {
            Vector2Int step = path[i];
            if (_hasDestination && !_map.IsWalkable(step.x, step.y))
            {
                List<Vector2Int> newPath = AStarPathfinder.FindPath(
                    _map.Height, _map.Width,
                    (r, c) => _map.IsWalkable(r, c),
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

            Vector3 target = _map.CellToWorld(step.x, step.y);
            while ((transform.position - target).sqrMagnitude > _arriveEps * _arriveEps)
            {
                if (_hasDestination && !_map.IsWalkable(step.x, step.y))
                    break;

                Vector3 prev = transform.position;
                transform.position = Vector3.MoveTowards(transform.position, target, _moveSpeed * Time.deltaTime);

                if (_monster != null)
                {
                    Vector3 delta = transform.position - prev;
                    _monster.SetFlip(delta);
                }
                yield return null;
            }

            if ((transform.position - target).sqrMagnitude > _arriveEps * _arriveEps)
            {
                i--;
                continue;
            }

            transform.position = target;
            Cell = step;
        }

        IsFollowingPath = false;
        _moveCo = null;
    }
}
