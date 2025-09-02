using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class BossController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BossMonster _boss;    
    [SerializeField] private TestMap _map;        
    [SerializeField] private MonsterManager _monsterManager; 

    [Header("Spawn Fallback")]
    [SerializeField] private Vector2Int _fallbackRC = new Vector2Int(0, 0);

    [Header("AI / Skills")]
    [SerializeField] private float _thinkInterval = 0.25f;
    [SerializeField] private bool _weightedRandom = true;
    [SerializeField] private List<BossSkillBase> _skills = new List<BossSkillBase>();

    private Coroutine _aiCo;

    public TestMap Map => _map;
    public MonsterManager MonsterManager => _monsterManager;
    public BossMonster Boss => _boss;

    private void Awake()
    {
        if (!_boss) _boss = GetComponent<BossMonster>();
        if (!_map) _map = FindAnyObjectByType<TestMap>();
        if (!_monsterManager) _monsterManager = FindAnyObjectByType<MonsterManager>();

        MonsterMover mover = GetComponent<MonsterMover>();
        if (mover) mover.enabled = false;
    }

    private void Start()
    {
        Vector2Int rc = _fallbackRC;

        if (_map)
        {
            if (_map.HasBossSpawnCell) rc = _map.BossSpawnCellRC;
            else if (_map.HasSpawnCell) rc = _map.SpawnCellRC;
        }

        if (_map) transform.position = _map.CellToWorld(rc.x, rc.y);

        foreach (var s in _skills) if (s) s.Setup(this);

        if (_skills.Count > 0)
            _aiCo = StartCoroutine(AI_Loop());
    }

    private IEnumerator AI_Loop()
    {
        WaitForSeconds wait = new WaitForSeconds(_thinkInterval);

        while (_boss && !_boss.IsDead)
        {
            List<BossSkillBase> candidates = new List<BossSkillBase>();
            for (int i = 0; i < _skills.Count; i++)
            {
                BossSkillBase s = _skills[i];
                if (s != null && s.CanCast(this)) candidates.Add(s);
            }

            if (candidates.Count > 0)
            {
                BossSkillBase chosen = _weightedRandom ? PickWeighted(candidates) : candidates[0];
                _boss?.FireAttackTrigger();

                yield return StartCoroutine(chosen.Execute(this));
            }

            yield return wait;
        }
    }

    private BossSkillBase PickWeighted(List<BossSkillBase> list)
    {
        float sum = 0f;
        foreach (var s in list) sum += Mathf.Max(0.001f, s.Weight);
        float r = Random.value * sum;
        foreach (var s in list)
        {
            r -= Mathf.Max(0.001f, s.Weight);
            if (r <= 0f) return s;
        }
        return list[list.Count - 1];
    }
}
