using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;

public sealed class StunUnit : BossSkillBase
{
    [Header("Stun Settings")]
    [SerializeField] private float _stunDuration = 3.0f;

    [SerializeField] private GameObject _stunVfxPrefab;
    [SerializeField] private Vector3 _worldOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private float _vfxLifetimeOverride = 0f;

    [SerializeField] private GameObject _preVfxPrefab;
    [SerializeField] private float _preDelay = 0.4f;
    [SerializeField] private float _preVfxLifetimeOverride = 0f;
    [SerializeField] private Vector3 _preWorldOffset = new Vector3(0f, 0.8f, 0f);

    [SerializeField] private int _targetsToStun = 3;

    protected override void Perform(BossController controller)
    {
        if (_map == null) { return; }

        List<Candidate> candidates = CollectAliveTowers();
        if (candidates.Count == 0) { return; }

        int k = Mathf.Clamp(_targetsToStun, 0, candidates.Count);
        if (k == 0) { return; }

        SelectKDistinctInPlace(candidates, k);


        MarkCastSuccess();

        for (int i = 0; i < k; i++)
        {
            StartCoroutine(DoStunSequence(candidates[i], controller));
        }
    }


    private static void SelectKDistinctInPlace<T>(List<T> list, int k)
    {
        for (int i = 0; i < k; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private IEnumerator DoStunSequence(Candidate pick, BossController controller)
    {
        int layerId = SortingLayer.NameToID("Unit");
        int order = 0;

        if (pick.Unit != null)
        {
            SortingGroup sg = pick.Unit.GetComponent<SortingGroup>();
            if (sg != null)
            {
                layerId = sg.sortingLayerID;
                order = sg.sortingOrder;
            }
            else
            {
                SpriteRenderer sr = pick.Unit.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    layerId = sr.sortingLayerID;
                    order = sr.sortingOrder;
                }
            }
        }

        GameObject preGo = null;
        if (_preVfxPrefab != null)
        {
            float preLife = (_preVfxLifetimeOverride > 0f) ? _preVfxLifetimeOverride : Mathf.Max(0.01f, _preDelay);
            preGo = SpawnVfxAtRaw(
                _preVfxPrefab,
                pick.WorldPos + _preWorldOffset,
                preLife,
                layerId,
                order + 9
            );
        }

        if (_preDelay > 0f)
            yield return new WaitForSeconds(_preDelay);

        if (pick.Unit == null || pick.Unit.IsDie) yield break;

        pick.Unit.StopAttack(_stunDuration);

        if (_stunVfxPrefab != null)
        {
            float life = (_vfxLifetimeOverride > 0f) ? _vfxLifetimeOverride : _stunDuration;
            SpawnVfxAtRaw(
                _stunVfxPrefab,
                pick.WorldPos + _worldOffset,
                life,
                layerId,
                order + 10
            );
        }
    }

    private struct Candidate
    {
        public Unit Unit;
        public Vector3 WorldPos;
    }

    private List<Candidate> CollectAliveTowers()
    {
        List<Candidate> list = new List<Candidate>();

        for (int r = 0; r < _map.Height; r++)
            for (int c = 0; c < _map.Width; c++)
            {
                if (!_map.HasTower(r, c)) continue;

                Vector3 world = _map.CellToWorld(r, c);
                Vector3Int abs = MapManager.Instance.WorldToCell(world);

                if (MapManager.Instance.TryGetTowerAt(abs, out GameObject towerGo) && towerGo)
                {
                    Unit u = towerGo.GetComponent<Unit>();
                    if (u != null && !u.IsDie)
                    {
                        list.Add(new Candidate { Unit = u, WorldPos = world });
                    }
                }
            }

        return list;
    }

    private GameObject SpawnVfxAtRaw(GameObject prefab, Vector3 worldPos, float life, int sortingLayerId, int sortingOrder)
    {
        if (prefab == null) return null;

        GameObject go = Instantiate(prefab, worldPos, Quaternion.identity);

        Vector3 p = go.transform.position;
        go.transform.position = new Vector3(p.x, p.y, 0f);

        ApplySort(go, sortingLayerId, sortingOrder);

        if (life > 0f) Destroy(go, life);
        return go;
    }

    private void ApplySort(GameObject root, int sortingLayerId, int sortingOrder)
    {
        SpriteRenderer[] srs = root.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < srs.Length; i++)
        {
            srs[i].sortingLayerID = sortingLayerId;
            srs[i].sortingOrder = sortingOrder;
        }

        ParticleSystemRenderer[] prs = root.GetComponentsInChildren<ParticleSystemRenderer>(true);
        for (int i = 0; i < prs.Length; i++)
        {
            prs[i].sortingLayerID = sortingLayerId;
            prs[i].sortingOrder = sortingOrder;
        }
    }

    public override bool CanCast(BossController controller)
    {
        if (_map == null) { return false; }
        // 대상(살아있는 타워) 1개 이상 있으면 가능
        List<Candidate> list = CollectAliveTowers();
        return list.Count > 0;
    }
}