using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MonsterMover : MonoBehaviour
{
    [SerializeField] private TestMap _map;
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _arriveEps = 0.02f;

    [SerializeField] private int _attackRangeCell = 1;
    [SerializeField] private bool _useDiag = false;
    [SerializeField] private float _attackCooldown = 1.0f;
    private float _attackTimer = 0f;
    private RouteManager _route;

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
    private bool _allowDestructible = false;

    private void Start()
    {
        if (!_map) _map = FindAnyObjectByType<TestMap>();
        if (!_route) _route = FindAnyObjectByType<RouteManager>();
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

        if (CheckGoalRange())
        {
            if (IsFollowingPath)
            {
                IsFollowingPath = false;
                _hasDestination = false;
                if (_moveCo != null) { StopCoroutine(_moveCo); _moveCo = null; }
            }

            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f && _monster.IsAttackReady())
            {
                _monster?.FireAttackTrigger();
                _attackTimer = _attackCooldown;
            }
        }
        else
            _attackTimer = 0f;
    }

    public void MoveToWorld(Vector3 world)
    {
        Vector2Int dst = _map.WorldToCell(world);
        MoveToCell(dst);
    }

    public void MoveToCell(Vector2Int dst, bool allowDestructible = false)
    {
        _attackTimer = 0;

        _dstCell = dst;
        _allowDestructible = allowDestructible;
        _hasDestination = true;

        //List<Vector2Int> path = AStarPathfinder.FindPath(
        //    _map.Height, _map.Width,
        //    (r, c) => _map.IsWalkable(r, c),
        //    Cell, _dstCell
        //);

        List<Vector2Int> path = AStarPathfinder.FindPath(
        _map.Height, _map.Width,
        (r, c) => _allowDestructible
                  ? (_map.IsWalkable(r, c) || _map.IsDestructible(r, c))
                  : _map.IsWalkable(r, c),
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

            if (CheckGoalRange())
            {
                IsFollowingPath = false;
                _hasDestination = false;
                _moveCo = null;
                yield break;
            }

            Vector2Int step = path[i];

            if (_allowDestructible && _map.IsDestructible(step.x, step.y))
            {
                if (_monster != null && _monster.IsAttackReady()) 
                    _monster.FireAttackTrigger();

                yield return new WaitUntil(() => _monster != null && _monster.IsAttackReady());
                _map.SetDestructible(step.x, step.y, false);
                i--;
                continue;
            }

            if (_hasDestination && !IsPassable(step.x, step.y))
            {
                List<Vector2Int> newPath = AStarPathfinder.FindPath(
                    _map.Height, _map.Width,
                    (r, c) => IsPassable(r, c), 
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
                if (_hasDestination && !IsPassable(step.x, step.y))
                    break;

                Vector3 prev = transform.position;
                transform.position = Vector3.MoveTowards(transform.position, target, _moveSpeed * Time.deltaTime);

                if (_monster != null)
                {
                    Vector3 delta = transform.position - prev;

                    if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                    {
                        Vector3 goalWorld = _map.CellToWorld(_dstCell.x, _dstCell.y);
                        float xdir = goalWorld.x - transform.position.x;
                        _monster.SetFlip(new Vector3(xdir, 0f, 0f));
                    }
                    else
                        _monster.SetFlip(delta);
                }

                if (CheckGoalRange())
                {
                    IsFollowingPath = false;
                    _hasDestination = false;
                    _moveCo = null;
                    yield break;
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

    private bool CheckGoalRange()
    {
        if (_route == null) return false;
        Vector2Int goal = _route.GoalCell;
        int dr = Mathf.Abs(Cell.x - goal.x);
        int dc = Mathf.Abs(Cell.y - goal.y);
        return _useDiag ? (Mathf.Max(dr, dc) <= _attackRangeCell): ((dr + dc) <= _attackRangeCell);
    }

    private bool IsPassable(int r, int c)
    {
        return _allowDestructible
            ? (_map.IsWalkable(r, c) || _map.IsDestructible(r, c))
            : _map.IsWalkable(r, c);
    }
}
