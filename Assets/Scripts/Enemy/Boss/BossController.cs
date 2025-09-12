using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class BossController : MonoBehaviour
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

    [Header("Casting Locks / Cooldowns")]
    [Tooltip("스킬 시전 중에는 다른 스킬 금지")]
    [SerializeField] private bool _lockWhileCasting = true;

    [Tooltip("어떤 스킬이든 '성공적으로 시전된' 이후, 전역적으로 스킬을 금지하는 시간(초)")]
    [SerializeField] private float _globalNoCastSeconds = 2.0f;

    private Coroutine _aiCo;
    private bool _isCasting = false;
    private BossSkillBase _currentSkill = null;

    // 전역 노캐스트 윈도우 종료 시각
    private float _globalNoCastUntil = 0f;

    // 내부 플래그: 방금 캐스트 성공 여부
    private bool _lastCastSuccess = false;

    public TestMap Map { get { return _map; } }
    public MonsterManager MonsterManager { get { return _monsterManager; } }
    public BossMonster Boss { get { return _boss; } }
    public bool IsCasting { get { return _isCasting; } }
    public BossSkillBase CurrentSkill { get { return _currentSkill; } }

    private void Awake()
    {
        if (_boss == null) { _boss = GetComponent<BossMonster>(); }
        if (_map == null) { _map = FindAnyObjectByType<TestMap>(); }
        if (_monsterManager == null) { _monsterManager = FindAnyObjectByType<MonsterManager>(); }

        MonsterMover mover = GetComponent<MonsterMover>();
        if (mover != null) { mover.enabled = false; }
    }

    private void Start()
    {
        if (_map == null) { return; }

        if (_map.HasBossSpawnCell)
        {
            Vector2Int rc = _map.BossSpawnCellRC;
            transform.position = _map.CellToWorld(rc.x, rc.y);
        }
        else
        {
            gameObject.SetActive(false);
            return;
        }

        for (int i = 0; i < _skills.Count; i++)
        {
            BossSkillBase s = _skills[i];
            if (s != null) { s.Setup(this); }
        }

        if (_skills.Count > 0 && _aiCo == null)
        {
            _aiCo = StartCoroutine(AI_Loop());
        }
    }

    private IEnumerator AI_Loop()
    {
        WaitForSeconds waitThink = new WaitForSeconds(_thinkInterval);

        while (_boss != null && !_boss.IsDead)
        {
            // 시전 중이면 대기
            if (_lockWhileCasting && _isCasting)
            {
                yield return waitThink;
                continue;
            }

            // 전역 노캐스트 윈도우가 열려 있으면 대기
            if (Time.time < _globalNoCastUntil)
            {
                yield return waitThink;
                continue;
            }

            // 후보: 스킬 자체 CanCast 통과만 사용(쿨은 스킬이 관리)
            List<BossSkillBase> candidates = new List<BossSkillBase>();
            for (int i = 0; i < _skills.Count; i++)
            {
                BossSkillBase s = _skills[i];
                if (s == null) { continue; }
                if (s.CanCast(this)) { candidates.Add(s); }
            }

            if (candidates.Count > 0)
            {
                List<BossSkillBase> order = _weightedRandom ? BuildWeightedOrder(candidates) : candidates;

                for (int i = 0; i < order.Count; i++)
                {
                    BossSkillBase skill = order[i];

                    // 시전 직전 재확인(환경 변화 반영)
                    if (Time.time < _globalNoCastUntil) { break; }
                    if (!skill.CanCast(this)) { continue; }

                    _lastCastSuccess = false;
                    yield return StartCoroutine(CastSkillCo(skill));

                    if (_lastCastSuccess)
                    {
                        break; // 성공했으면 더 시도하지 않음
                    }
                    // 실패면(효과 미발생) 전역 대기 없이 다음 후보를 같은 틱에서 시도
                }
            }

            yield return waitThink;
        }
    }

    private IEnumerator CastSkillCo(BossSkillBase skill)
    {
        _lastCastSuccess = false;

        // JIT 재확인: 불가면 즉시 종료(대기 없음)
        if (!skill.CanCast(this))
        {
            yield break;
        }

        _isCasting = true;
        _currentSkill = skill;

        // 선택 스킬 외 모두 비활성화(애니 이벤트 통한 오발동 차단)
        SetOtherSkillsEnabled(false, skill);

        if (_boss != null)
        {
            _boss.FireAttackTrigger();
        }

        // BossSkillBase.Execute 내부에서 ResetCastOutcome() 호출 + Perform에서 MarkCastSuccess()
        IEnumerator exec = skill.Execute(this);
        if (exec != null)
        {
            yield return StartCoroutine(exec);
        }

        // 성공했을 때만 전역 노캐스트 창 부여
        if (skill.LastCastSucceeded)
        {
            _globalNoCastUntil = Time.time + Mathf.Max(0.0f, _globalNoCastSeconds);
            _lastCastSuccess = true;
        }

        // 복구
        SetOtherSkillsEnabled(true, skill);
        _currentSkill = null;
        _isCasting = false;
    }

    private void SetOtherSkillsEnabled(bool enabled, BossSkillBase except)
    {
        for (int i = 0; i < _skills.Count; i++)
        {
            BossSkillBase s = _skills[i];
            if (s == null) { continue; }
            if (s == except) { continue; }
            s.enabled = enabled;
        }
    }

    // 후보를 "가중치 무작위, 중복 없이" 순열로 변환
    private List<BossSkillBase> BuildWeightedOrder(List<BossSkillBase> src)
    {
        List<BossSkillBase> pool = new List<BossSkillBase>(src.Count);
        for (int i = 0; i < src.Count; i++) { pool.Add(src[i]); }

        List<BossSkillBase> order = new List<BossSkillBase>(src.Count);

        while (pool.Count > 0)
        {
            float sum = 0f;
            for (int i = 0; i < pool.Count; i++)
            {
                sum += Mathf.Max(0.001f, pool[i].Weight);
            }

            float r = Random.value * sum;
            int pickedIndex = 0;
            for (int i = 0; i < pool.Count; i++)
            {
                r -= Mathf.Max(0.001f, pool[i].Weight);
                if (r <= 0f)
                {
                    pickedIndex = i;
                    break;
                }
            }

            BossSkillBase picked = pool[pickedIndex];
            order.Add(picked);
            pool.RemoveAt(pickedIndex);
        }

        return order;
    }
}
