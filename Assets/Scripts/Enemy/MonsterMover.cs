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

    public TestMap Map
    {
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

    private bool _allowWalls = false;
    private bool _allowTowers = false;

    private void Start()
    {
        if (!_map) _map = FindAnyObjectByType<TestMap>();
        if (!_route) _route = FindAnyObjectByType<RouteManager>();
        _monster = GetComponent<Monster>();

        Vector2Int rc = _map.WorldToCell(transform.position);
        Cell = rc;
        transform.position = _map.CellToWorld(rc.x, rc.y);
    }


    public void MoveToCell(Vector2Int dst, bool allowWalls , bool allowTowers)
    {
        _attackTimer = 0;
        _dstCell = dst;
        _allowWalls = allowWalls;
        _allowTowers = allowTowers;
        _hasDestination = true;

        List<Vector2Int> path = AStarPathfinder.FindPath(
            _map.Height, _map.Width,
            (r, c) => IsPassableOrTarget(r, c),
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

    private bool IsPassableOrTarget(int r, int c)
    {
        
        if (_map.IsWalkable(r, c)) return true;
        if (_allowWalls && _map.IsDestructible(r, c)) return true;
        if (_allowTowers && _map.HasTower(r, c)) return true;
        return false;
    }

    private bool IsPassableOnly(int r, int c)
    {
        return _map.IsWalkable(r, c);
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

            if ((_allowWalls && _map.IsDestructible(step.x, step.y)) ||(_allowTowers && _map.HasTower(step.x, step.y)))
            {
                if (_monster != null)
                {
                    yield return new WaitUntil(() => _monster.IsAttackReady()); // 준비 대기
                    _monster.FireAttackTrigger();                                // 트리거
                    yield return new WaitUntil(() => _monster.IsAttackReady()); // 종료 대기
                }

                if (_allowWalls && _map.IsDestructible(step.x, step.y))
                    _map.SetDestructible(step.x, step.y, false);
                else if (_allowTowers && _map.HasTower(step.x, step.y))
                {
                    Vector3 targetWorld = _map.CellToWorld(step.x, step.y);
                    Vector3Int abs = MapManager.Instance.WorldToCell(targetWorld);

                    if (MapManager.Instance.TryGetTowerAt(abs, out GameObject towerGo) && towerGo)
                    {
                        var unit = towerGo.GetComponent<Unit>();
                        if (unit != null && !unit.IsDie)
                        {
                            unit.TakeDamage(1);  

                            i--;
                            continue;
                        }
                    }
                }
                    
                i--;
                continue;
            }


            if (_hasDestination && !IsPassableOnly(step.x, step.y))
            {
                List<Vector2Int> newPath = AStarPathfinder.FindPath(
                    _map.Height, _map.Width, (r, c) => IsPassableOrTarget(r, c),Cell, _dstCell
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
                if (_hasDestination && !IsPassableOnly(step.x, step.y))
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

    private void Update()
    {
        if (CheckGoalRange())
        {
            if (IsFollowingPath)
            {
                IsFollowingPath = false;
                _hasDestination = false;
                if (_moveCo != null) { StopCoroutine(_moveCo); _moveCo = null; }
            }

            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f && _monster != null && _monster.IsAttackReady())
            {
                _monster?.FireAttackTrigger();
                _attackTimer = _attackCooldown;
            }
        }
        else
            _attackTimer = 0f;
    }
}
