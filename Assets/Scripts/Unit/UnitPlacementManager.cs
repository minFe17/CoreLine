using UnityEngine;

public class UnitPlacementManager
{
    // ╫л╠шео
    public Vector3 GetMouseWorldPosition()
    {
        Vector3 pos = Input.mousePosition;
        Vector3 world = Camera.main.ScreenToWorldPoint(pos);
        world.z = 0f;
        return world;
    }

    public Vector3Int GetCellFromWorld(Vector3 worldPos)
    {
        return MapManager.Instance.GetPlaceInfoWorld(worldPos).Cell;
    }

    public Vector3 GetCellCenter(Vector3Int cell)
    {
        return MapManager.Instance.GetCellCenterWorld(cell);
    }

    public Vector3Int GetMouseCell()
    {
        return GetCellFromWorld(GetMouseWorldPosition());
    }

    public Vector3 GetMouseCellCenter()
    {
        return GetCellCenter(GetMouseCell());
    }
}