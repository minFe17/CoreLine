using UnityEngine;
using UnityEngine.Tilemaps;

public class DestructibleWall : MonoBehaviour
{
    private MapManager _map;
    private Tilemap _destructible;

    public void Init(MapManager map, Tilemap destructible)
    {
        _map = map;
        _destructible = destructible;
    }

    public void DestroyAtCell(Vector3Int cell)
    {
        if (_map == null || _destructible == null) return;
        _map.ConvertDestructibleToBuildable(cell);
    }

    // ÇÊ¿äÇÏ¸é ¿ùµåÁÂÇ¥ ¡æ ¼¿ º¯È¯ ÇïÆÛ
    public void DestroyAtWorld(Vector3 worldPos)
    {
        var cell = _destructible.WorldToCell(worldPos);
        DestroyAtCell(cell);
    }
}
