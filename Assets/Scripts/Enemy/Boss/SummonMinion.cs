using System.Collections.Generic;
using UnityEngine;

public class SummonMinions : BossSkillBase
{
    [Header("Summon")]
    [SerializeField] private MonsterMover _minionPrefab;
    [SerializeField] private int _count = 3;               // 소환 수
    [SerializeField] private int _maxTrialsPerSpawn = 40;  // 한 마리 뽑을 때 최대 시도

    [Header("Placement Rules")]
    [SerializeField] private float _minDistFromBoss = 1.0f;   // 보스로부터 최소 거리(월드)
    [SerializeField] private float _minDistFromBase = 2.0f;   // 베이스로부터 최소 거리(월드)
    [SerializeField] private bool _spawnOnBuildableOnly = true;

    [Header("Telegraph (optional)")]
    [SerializeField] private GameObject _telegraphPrefab;  // 바닥 예고 표시 프리팹(원, 십자 등)
    [SerializeField] private float _telegraphLifetime = 0.9f;
    [SerializeField] private string _telegraphSortingLayer = "Effects";
    [SerializeField] private int _telegraphOrderInLayer = 100;

    // Execute()는 BossSkillBase가 제공(캐스팅시간/후딜/쿨다운 관리)
    // Perform()만 구현하면 된다.
    protected override void Perform(BossController controller)
    {
        if (_map == null || _minionPrefab == null) { return; }

        List<Vector2Int> targets = PickSpawnCells(_count);
        if (targets == null || targets.Count == 0) { return; }

        MarkCastSuccess();

        foreach (Vector2Int rc in targets)
        {
            Vector3 world = _map.CellToWorld(rc.x, rc.y);
            world.z = 0f;

            if (_telegraphPrefab != null)
            {
                GameObject g = Object.Instantiate(_telegraphPrefab, world, Quaternion.identity);
                SetupSortingLayer(g, _telegraphSortingLayer, _telegraphOrderInLayer);
                Object.Destroy(g, _telegraphLifetime);
            }

            if (_controller != null && _controller.MonsterManager != null)
            {
                _controller.MonsterManager.SpawnAtWorld(_minionPrefab, world, sendToGoal: true);
            }
        }
    }


    private List<Vector2Int> PickSpawnCells(int want)
    {
        var list = new List<Vector2Int>(want);
        var used = new HashSet<int>();
        int trials = 0;
        int maxTrials = Mathf.Max(want * _maxTrialsPerSpawn, want);

        Vector3 bossPos = _boss ? _boss.transform.position : Vector3.zero;
        Vector3 basePos = Vector3.positiveInfinity;
        if (MapManager.Instance && MapManager.Instance.HasPlayerBase)
            basePos = MapManager.Instance.CellCenterWorld(MapManager.Instance.PlayerBaseCell);

        float minBoss2 = _minDistFromBoss * _minDistFromBoss;
        float minBase2 = _minDistFromBase * _minDistFromBase;

        while (list.Count < want && trials++ < maxTrials)
        {
            int r = Random.Range(0, _map.Height);
            int c = Random.Range(0, _map.Width);

            if (!_map.IsBuildable(r, c)) continue;

            int key = (r << 16) ^ c;
            if (used.Contains(key)) continue;

            Vector3 w = _map.CellToWorld(r, c);
            if ((w - bossPos).sqrMagnitude < minBoss2) continue;
            if (basePos.x < 1e8f && (w - basePos).sqrMagnitude < minBase2) continue;

            used.Add(key);
            list.Add(new Vector2Int(r, c));
        }
        return list;
    }

    private void SetupSortingLayer(GameObject go, string layer, int order)
    {
        foreach (var sr in go.GetComponentsInChildren<SpriteRenderer>(true))
        {
            sr.sortingLayerName = layer;
            sr.sortingOrder = order;
        }
        foreach (var psr in go.GetComponentsInChildren<ParticleSystemRenderer>(true))
        {
            psr.sortingLayerName = layer;
            psr.sortingOrder = order;
        }
        var canvas = go.GetComponentInChildren<Canvas>(true);
        if (canvas) { canvas.sortingLayerName = layer; canvas.sortingOrder = order; }
    }

    public override bool CanCast(BossController controller)
    {
        if (_map == null) { return false; }
        if (_minionPrefab == null) { return false; }

        // 최소 1곳이라도 조건을 만족하는 셀 있으면 허용
        List<Vector2Int> one = PickSpawnCells(1);
        return one.Count > 0;
    }

}