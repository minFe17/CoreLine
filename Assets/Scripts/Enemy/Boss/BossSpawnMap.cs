using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering; 

public sealed class BossSpawnMap : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float _delaySeconds = 10f;

    [Header("References")]
    [SerializeField] private BossController _controller;
    [SerializeField] private TestMap _map;
    [SerializeField] private RouteManager _route;

    [Header("Spawn (Always at Normal Spawn Cell)")]
    [SerializeField] private MonsterMover _normalMonsterPrefab;
    [SerializeField] private Vector2Int _fallbackSpawnCell = new Vector2Int(0, 0);

    [Header("Copy Stats")]
    [SerializeField] private bool _copyHpToNew = true;
    [SerializeField] private int _minHpFallback = 10;

    [Header("Despawn VFX (Boss Removed) - MAIN")]
    [SerializeField] private GameObject _despawnVfxPrefab;
    [SerializeField] private Vector3 _despawnVfxOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private float _despawnVfxLifetime = 1.5f;

    [Header("Despawn VFX (Boss Removed) - EXTRA")]
    [SerializeField] private GameObject _despawnVfxPrefab2;             
    [SerializeField] private Vector3 _despawnVfxOffset2 = new Vector3(0f, 0.6f, 0f);
    [SerializeField] private float _despawnVfxLifetime2 = 1.5f;

    [Header("Despawn VFX Timing")]
    [SerializeField, Tooltip("교체 시점보다 얼마나 일찍 디스폰 VFX를 재생할지(초)")]
    private float _despawnVfxAdvance = 0.25f;

    private void Awake()
    {
        if (_controller == null) { _controller = Object.FindAnyObjectByType<BossController>(); }
        if (_map == null) { _map = Object.FindAnyObjectByType<TestMap>(); }
        if (_route == null) { _route = Object.FindAnyObjectByType<RouteManager>(); }
    }

    private void OnEnable()
    {
        StartCoroutine(CoDelayThenReplace());
    }

    private IEnumerator CoDelayThenReplace()
    {
        float advance = Mathf.Max(0f, _despawnVfxAdvance);
        float waitBeforeVfx = Mathf.Max(0f, _delaySeconds - advance);
        float waitAfterVfx = Mathf.Max(0f, _delaySeconds - waitBeforeVfx); 
        if (waitBeforeVfx > 0f)
            yield return new WaitForSeconds(waitBeforeVfx);

      
        PlayDespawnVfxNow();

        if (waitAfterVfx > 0f)
            yield return new WaitForSeconds(waitAfterVfx);

        ReplaceWithPrefabFlow(skipDespawnVfx: true);
    }

    private void ReplaceWithPrefabFlow(bool skipDespawnVfx = false)
    {
        if (_normalMonsterPrefab == null) { return; }
        if (_map == null) { return; }

        
        if (!skipDespawnVfx && _despawnVfxPrefab != null)
        {
            Vector3 bossWorldNow = transform.position;
            SpawnCustomVfxAt(_despawnVfxPrefab, bossWorldNow + _despawnVfxOffset, _despawnVfxLifetime, gameObject);
            if (_despawnVfxPrefab2 != null)
                SpawnCustomVfxAt(_despawnVfxPrefab2, bossWorldNow + _despawnVfxOffset2, _despawnVfxLifetime2, gameObject);
        }

        
        Vector2Int spawnCell = ResolveNormalSpawnCell();
        Vector3 spawnWorld = _map.CellToWorld(spawnCell.x, spawnCell.y);
        spawnWorld.z = 0f;

      
        MonsterMover mover = null;
        if (_controller != null && _controller.MonsterManager != null)
        {
            mover = _controller.MonsterManager.SpawnAtWorld(_normalMonsterPrefab, spawnWorld, true);
        }
        else
        {
            GameObject go = Object.Instantiate(_normalMonsterPrefab.gameObject, spawnWorld, Quaternion.identity);
            mover = go.GetComponent<MonsterMover>();
            if (mover != null)
            {
                mover.Map = _map;
                mover.SetCellAndSnap(spawnCell);

                RouteManager route = _route != null ? _route : Object.FindAnyObjectByType<RouteManager>();
                if (route != null)
                {
                    Vector2Int goal = route.GoalCell;
                    mover.MoveToCell(goal, false, false);
                }
            }
        }

        
        if (mover != null && _copyHpToNew)
        {
            HealthComponent to = mover.GetComponent<HealthComponent>();
            if (to != null)
            {
                int need = to.MaxHp - to.CurrentHp;
                if (need > 0) { to.Heal(need); }
            }
        }

        

        gameObject.SetActive(false);
        Object.Destroy(gameObject);
    }

    private void PlayDespawnVfxNow()
    {
        Vector3 bossWorld = transform.position; 
        if (_despawnVfxPrefab != null)
        {
            SpawnCustomVfxAt(_despawnVfxPrefab, bossWorld + _despawnVfxOffset, _despawnVfxLifetime, gameObject);
        }
        if (_despawnVfxPrefab2 != null)
        {
            SpawnCustomVfxAt(_despawnVfxPrefab2, bossWorld + _despawnVfxOffset2, _despawnVfxLifetime2, gameObject);
        }
    }

    private Vector2Int ResolveNormalSpawnCell()
    {
        if (_route != null)
        {
            FieldInfo fSpawn = _route.GetType().GetField("SpawnCell", BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo pSpawn = _route.GetType().GetProperty("SpawnCell", BindingFlags.Public | BindingFlags.Instance);
            if (fSpawn != null && fSpawn.FieldType == typeof(Vector2Int))
                return (Vector2Int)fSpawn.GetValue(_route);
            if (pSpawn != null && pSpawn.PropertyType == typeof(Vector2Int))
                return (Vector2Int)pSpawn.GetValue(_route, null);

            FieldInfo fStart = _route.GetType().GetField("StartCell", BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo pStart = _route.GetType().GetProperty("StartCell", BindingFlags.Public | BindingFlags.Instance);
            if (fStart != null && fStart.FieldType == typeof(Vector2Int))
                return (Vector2Int)fStart.GetValue(_route);
            if (pStart != null && pStart.PropertyType == typeof(Vector2Int))
                return (Vector2Int)pStart.GetValue(_route, null);
        }
        return _fallbackSpawnCell;
    }

    
    private void SpawnCustomVfxAt(GameObject prefab, Vector3 worldPos, float life, GameObject targetForSorting)
    {
        if (prefab == null) return;

        Quaternion rot = prefab.transform.rotation;
        Vector3 scl = prefab.transform.localScale;

        GameObject go = Object.Instantiate(prefab, worldPos, rot);
        go.transform.localScale = scl;

       
        Vector3 p = go.transform.position;
        go.transform.position = new Vector3(p.x, p.y, 0f);

        int layerId = SortingLayer.NameToID("Default");
        int order = 0;
        if (targetForSorting != null)
        {
            var sg = targetForSorting.GetComponent<SortingGroup>();
            if (sg != null) { layerId = sg.sortingLayerID; order = sg.sortingOrder; }
            else
            {
                var sr = targetForSorting.GetComponentInChildren<SpriteRenderer>();
                if (sr != null) { layerId = sr.sortingLayerID; order = sr.sortingOrder; }
            }
        }
        ApplySortingRecursively(go, layerId, order + 10);

        if (life > 0f) Object.Destroy(go, life);
    }

    private void ApplySortingRecursively(GameObject root, int sortingLayerId, int sortingOrder)
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
}
