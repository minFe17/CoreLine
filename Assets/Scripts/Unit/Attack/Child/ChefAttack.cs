using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utils;

public class ChefAttack : AttackBase
{
    private List<Tilemap> _targetTilemaps;

    void SetList()
    {
        _targetTilemaps = new List<Tilemap>
        {
            MapManager.Instance.BuildableTile,
            MapManager.Instance.UnbuildableTile
        };
    }

    public override void Attack()
    {
        float radius = _unit.UnitStateData.AttackRange;

        Vector3 randomPos;

        while (true)
        {
            // 공격 범위 내 랜덤 위치 (원 안에서)
            Vector2 randomCircle = Random.insideUnitCircle * radius;
            randomPos = transform.position + new Vector3(randomCircle.x, randomCircle.y, 0f);

            // 랜덤 위치를 타일맵 셀 좌표로 변환
            Vector3Int cellPos = _targetTilemaps[0].WorldToCell(randomPos);

            bool tileExists = false;
            foreach (Tilemap tilemap in _targetTilemaps)
            {
                if (tilemap.GetTile(cellPos) != null)
                {
                    tileExists = true;
                    break;
                }
            }

            if (tileExists)
            {
                GameObject temp = MonoSingleton<ObjectPoolManager>.Instance.Pull(EBulletType.ChefBomb);
                temp.transform.position = randomPos;
                if (_unit is TowerUnit unit)
                    temp.GetComponent<ChefBomb>().Init(unit.UnitStateData.AttackDamage, unit.Level);
                return;
            }
        }
    }

    protected override bool CheckAttack()
    {
        if (_targetTilemaps == null)
            SetList();
        if (_targetTilemaps == null || _targetTilemaps.Count == 0)
            return false;

        Vector3Int centerCell = _targetTilemaps[0].WorldToCell(transform.position);
        int radius = Mathf.CeilToInt(_unit.UnitStateData.AttackRange);

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int offset = new Vector3Int(x, y, 0);
                Vector3Int cellPos = centerCell + offset;

                Vector3 cellWorldPos = _targetTilemaps[0].GetCellCenterWorld(cellPos);
                float distance = Vector3.Distance(cellWorldPos, transform.position);

                if (distance > _unit.UnitStateData.AttackRange)
                    continue;

                foreach (Tilemap tilemap in _targetTilemaps)
                {
                    if (tilemap.GetTile(cellPos) != null)
                        return true;
                }
            }
        }
        return false;
    }
}