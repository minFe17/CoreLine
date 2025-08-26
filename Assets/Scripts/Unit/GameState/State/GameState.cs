using UnityEngine;
using Utils;

public class GameState : IState
{
    void HandleMouseClick()
    {
        Vector3 mouseWorld = SimpleSingleton<UnitPlacementManager>.Instance.GetMouseWorldPosition();
        Vector3Int cell = SimpleSingleton<UnitPlacementManager>.Instance.GetCellFromWorld(mouseWorld);
        Vector3 cellCenter = MapManager.Instance.GetCellCenterWorld(cell);

        CreateUI(cell, cellCenter);
        CreateUnit(cell, cellCenter);
    }

    // UI에서 호출?
    void CreateUnit(Vector3Int cell, Vector3 worldPosition)
    {
        // 임시
        MapManager.PlaceInfo temp = MapManager.Instance.GetPlaceInfo(cell);
        Debug.Log(temp.Occupied);
        if (!temp.Placeable || temp.Occupied)
            return;
        int randomIndex = Random.Range(1, (int)EUnitType.Max);


        GameObject unit = MonoSingleton<ObjectPoolManager>.Instance.Pull((EUnitType)randomIndex);
        unit.transform.position = worldPosition;

        MapManager.Instance.RegisterTower(cell, unit);
    }

    void CreateUI(Vector3Int cell, Vector3 worldPosition)
    {
        MapManager.PlaceInfo temp = MapManager.Instance.GetPlaceInfo(cell);
        Debug.Log(temp.Occupied);
        if (!temp.Placeable || temp.Occupied)
            return;

        // UI 띄우기
    }

    #region Interface
    void IState.Enter()
    {

    }

    void IState.Exit()
    {

    }

    void IState.Loop()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }
    #endregion
}