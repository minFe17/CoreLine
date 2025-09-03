using UnityEngine;

public class PooledMonster : MonoBehaviour
{
    [HideInInspector] public MonsterManager Manager;
    [HideInInspector] public MonsterMover PrefabKey; 

    private Monster _owner;

    private void Awake() => _owner = GetComponent<Monster>();

    private void OnDisable()
    {
        // 죽음/수동 비활성 모두 여기로 들어옴
        if (Manager && PrefabKey)
        {
            var mover = GetComponent<MonsterMover>();
            if (mover) Manager.DespawnToPool(mover, PrefabKey);
        }
    }
}
