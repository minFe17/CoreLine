using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public sealed class MonsterMover : MonoBehaviour
{
    [SerializeField] private TestMap _map;

    [Header("Move")]
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _arriveEps = 0.02f;
    [SerializeField] private float _minMoveSpeed = 0.5f;
    private float _baseMoveSpeed = 4f;

    private List<SpeedModifier> _speedMods = new List<SpeedModifier>();

    [Header("Attack To Base")]
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
    public float CurrentMoveSpeed { get { return _moveSpeed; } }

    private Vector2Int _dstCell;
    private bool _hasDestination;
    private Coroutine _moveCo;
    private Monster _monster;

    
    private IMoveStyle _style;

    private bool _allowWalls = false;
    private bool _allowTowers = false;

    
    public enum SpeedModSource { Skill = 0, Tile = 1, Boss = 2 }

    private struct SpeedModifier
    {
        public SpeedModSource Source;
        public float Delta;
        public float ExpiresAt;
    }

    private void Awake()
    {
        if (!_map) _map = FindAnyObjectByType<TestMap>();
        if (!_route) _route = FindAnyObjectByType<RouteManager>();
        _monster = GetComponent<Monster>();
        _style = GetComponent<IMoveStyle>();

        _baseMoveSpeed = Mathf.Max(_minMoveSpeed, _moveSpeed);
        _moveSpeed = _baseMoveSpeed;
    }

    private void Start()
    {
        if (_map)
        {
            Vector2Int rc = _map.WorldToCell(transform.position);
            Cell = rc;
            transform.position = _map.CellToWorld(rc.x, rc.y);
        }
    }

    private void OnEnable()
    {
        _speedMods.Clear();
        _moveSpeed = _baseMoveSpeed;
    }

   
    public void MoveToCell(Vector2Int dst, bool allowWalls, bool allowTowers)
    {
        if (!isActiveAndEnabled) { return; }

        _attackTimer = 0f;
        _dstCell = dst;
        _allowWalls = allowWalls;
        _allowTowers = allowTowers;
        _hasDestination = true;

        List<Vector2Int> path = AStarPathfinder.FindPath(
            _map.Height, _map.Width,
            (r, c) => IsPassableOrTarget(r, c) || (r == Cell.x && c == Cell.y),
            Cell, _dstCell
        );

        if (path == null || path.Count <= 1)
        {
            IsFollowingPath = false;
            _hasDestination = false;
            if (_moveCo != null) { StopCoroutine(_moveCo); _moveCo = null; }
            return;
        }

        if (_moveCo != null) { StopCoroutine(_moveCo); }
        IsFollowingPath = true;
        _moveCo = StartCoroutine(Follow(path));
    }

    public void OnOwnerDied()
    {
        if (_moveCo != null) { StopCoroutine(_moveCo); _moveCo = null; }
        IsFollowingPath = false;
    }

    public void SetCellAndSnap(Vector2Int rc)
    {
        Cell = rc;
        transform.position = _map.CellToWorld(rc.x, rc.y);
    }

    
    private bool IsPassableOrTarget(int r, int c)
    {
        if (_map.IsWalkable(r, c)) { return true; }
        if (_allowWalls && _map.IsDestructible(r, c)) { return true; }
        if (_allowTowers && _map.HasTower(r, c)) { return true; }
        return false;
    }

    private bool IsPassableOnly(int r, int c)
    {
        if (r == Cell.x && c == Cell.y) { return true; }
        return _map.IsWalkable(r, c);
    }

    private IEnumerator Follow(List<Vector2Int> path)
    {
        int i = (path[0] == Cell) ? 1 : 0;

        for (; i < path.Count; i++)
        {
            if (_monster != null && _monster.IsDead) { yield break; }

            if (CheckGoalRange())
            {
                IsFollowingPath = false;
                _hasDestination = false;
                _moveCo = null;
                yield break;
            }

            Vector2Int step = path[i];

            bool isUnderFeet = (step.x == Cell.x && step.y == Cell.y);
            if (!isUnderFeet &&
                (
                    (_allowWalls && _map.IsDestructible(step.x, step.y)) ||
                    (_allowTowers && _map.HasTower(step.x, step.y))
                ))
            {
                if (_monster != null)
                {
                    yield return new WaitUntil(() => _monster.IsAttackReady());
                    _monster.FireAttackTrigger();
                    yield return new WaitUntil(() => _monster.IsAttackReady());
                }

                if (_allowWalls && _map.IsDestructible(step.x, step.y))
                {
                    _map.SetDestructible(step.x, step.y, false);

                    List<Vector2Int> newPath = AStarPathfinder.FindPath(
                        _map.Height, _map.Width,
                        (r, c) => IsPassableOrTarget(r, c) || (r == Cell.x && c == Cell.y),
                        Cell, _dstCell
                    );
                    if (newPath != null && newPath.Count > 1)
                    {
                        path = newPath;
                        i = (path[0] == Cell) ? 1 : 0;
                    }
                    continue;
                }
                else if (_allowTowers && _map.HasTower(step.x, step.y))
                {
                    Vector3 targetWorld = _map.CellToWorld(step.x, step.y);
                    Vector3Int abs = MapManager.Instance.WorldToCell(targetWorld);

                    GameObject towerGo;
                    if (MapManager.Instance.TryGetTowerAt(abs, out towerGo) && towerGo)
                    {
                        Unit unit = towerGo.GetComponent<Unit>();
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
                    _map.Height, _map.Width,
                    (r, c) => IsPassableOrTarget(r, c) || (r == Cell.x && c == Cell.y),
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

            Vector3 baseTarget = _map.CellToWorld(step.x, step.y);

            while ((transform.position - baseTarget).sqrMagnitude > _arriveEps * _arriveEps)
            {
                if (_monster != null && _monster.IsDead) { yield break; }
                if (_hasDestination && !IsPassableOnly(step.x, step.y)) { break; }

                Vector3 prev = transform.position;

                Vector3 d = baseTarget - transform.position;
                float dLen = d.magnitude;
                Vector3 dir = (dLen > 1e-6f) ? (d / dLen) : Vector3.right;

                Vector3 t = baseTarget;
                float mul = 1f;

                if (_style != null)
                {
                    (Vector3 targetWorld, float speedMul) res =
                        _style.Tick(transform.position, baseTarget, dir, _moveSpeed, Time.deltaTime);

                    t = res.targetWorld;
                    mul = res.speedMul;

                    float r = _map.CellSize * 0.40f;
                    Vector3 a = transform.position;
                    Vector3 b = baseTarget;
                    Vector3 ab = b - a;
                    float ab2 = ab.sqrMagnitude;
                    if (ab2 > 1e-6f)
                    {
                        float u = Mathf.Clamp01(Vector3.Dot(t - a, ab) / ab2);
                        Vector3 p = a + ab * u;
                        Vector3 off = t - p;
                        float offLen = off.magnitude;
                        if (offLen > r) { t = p + (off / Mathf.Max(offLen, 1e-6f)) * r; }
                    }
                    else
                    {
                        Vector3 off = t - a;
                        float offLen = off.magnitude;
                        if (offLen > r) { t = a + (off / Mathf.Max(offLen, 1e-6f)) * r; }
                    }
                }

                float spd = _moveSpeed * Mathf.Max(0.05f, mul);
                transform.position = Vector3.MoveTowards(transform.position, t, spd * Time.deltaTime);

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
                    {
                        _monster.SetFlip(delta);
                    }
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

            if ((transform.position - baseTarget).sqrMagnitude > _arriveEps * _arriveEps)
            {
                i--;
                continue;
            }

            transform.position = baseTarget;
            Cell = step;
        }

        IsFollowingPath = false;
        _moveCo = null;
    }

    private bool CheckGoalRange()
    {
        if (_route == null) { return false; }
        Vector2Int goal = _route.GoalCell;
        int dr = Mathf.Abs(Cell.x - goal.x);
        int dc = Mathf.Abs(Cell.y - goal.y);
        return _useDiag ? (Mathf.Max(dr, dc) <= _attackRangeCell) : ((dr + dc) <= _attackRangeCell);
    }

    private void Update()
    {
        // 만료 즉시 반영
        RecomputeMoveSpeed(Time.time);

        if (_monster != null && _monster.IsDead) { return; }

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
                _monster.FireAttackTrigger();
                AttackUnitBase();
                _attackTimer = _attackCooldown;
            }
        }
        else
        {
            _attackTimer = 0f;
        }
    }

    private void AttackUnitBase()
    {
        if (_route == null || _map == null) { return; }
        if (MapManager.Instance == null || !MapManager.Instance.HasPlayerBase) { return; }

        Vector2Int goal = _route.GoalCell;
        Vector3 world = _map.CellToWorld(goal.x, goal.y);
        Vector3Int abs = MapManager.Instance.WorldToCell(world);

        GameObject towerGo;
        if (MapManager.Instance.TryGetTowerAt(abs, out towerGo) && towerGo)
        {
            Unit towerUnit = towerGo.GetComponent<Unit>();
            if (towerUnit != null && !towerUnit.IsDie)
            {
                towerUnit.TakeDamage(1);
                return;
            }
        }
    }

   
    // 소스 3개, 같은 값이면 시간 갱신 / 다른 값이면 스택 추가
    public void ApplySpeedModifier(SpeedModSource source, float deltaSpeed, float duration)
    {
        float now = Time.time;
        int idx = FindSameDeltaIndex(source, deltaSpeed, 0.0001f);

        if (idx >= 0)
        {
            SpeedModifier cur = _speedMods[idx];
            float newExpire = now + Mathf.Max(0.0f, duration);
            if (newExpire > cur.ExpiresAt) { cur.ExpiresAt = newExpire; }
            _speedMods[idx] = cur;
        }
        else
        {
            SpeedModifier m = new SpeedModifier
            {
                Source = source,
                Delta = deltaSpeed,
                ExpiresAt = now + Mathf.Max(0.0f, duration)
            };
            _speedMods.Add(m);
        }

        RecomputeMoveSpeed(now);
    }

    public void ClearSpeedModifiersFromSource(SpeedModSource source)
    {
        bool removed = false;
        for (int i = _speedMods.Count - 1; i >= 0; i--)
        {
            if (_speedMods[i].Source == source)
            {
                _speedMods.RemoveAt(i);
                removed = true;
            }
        }
        if (removed) { RecomputeMoveSpeed(Time.time); }
    }

    public void ClearAllSpeedModifiers()
    {
        _speedMods.Clear();
        _moveSpeed = _baseMoveSpeed;
    }

    private void RecomputeMoveSpeed(float now)
    {
        // 만료 제거
        for (int i = _speedMods.Count - 1; i >= 0; i--)
        {
            if (_speedMods[i].ExpiresAt <= now)
            {
                _speedMods.RemoveAt(i);
            }
        }

        // 합산
        float sum = 0f;
        for (int i = 0; i < _speedMods.Count; i++)
        {
            sum += _speedMods[i].Delta;
        }

        _moveSpeed = Mathf.Max(_minMoveSpeed, _baseMoveSpeed + sum);
    }

    private int FindSameDeltaIndex(SpeedModSource source, float delta, float eps)
    {
        for (int i = 0; i < _speedMods.Count; i++)
        {
            if (_speedMods[i].Source == source && Mathf.Abs(_speedMods[i].Delta - delta) <= eps)
            {
                return i;
            }
        }
        return -1;
    }
}
