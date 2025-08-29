using UnityEngine;
using Utils;

public class SelectKingTileState : IState
{
    void HandleMouseClick()
    {
        Vector3 mouseWorld = SimpleSingleton<UnitPlacementManager>.Instance.GetMouseWorldPosition();
        Vector3Int cell = SimpleSingleton<UnitPlacementManager>.Instance.GetCellFromWorld(mouseWorld);
        Vector3 cellCenter = MapManager.Instance.GetCellCenterWorld(cell);

        if (TryCreateKing(cell, cellCenter))
        {
            ChangeToGameState();
        }
    }

    bool TryCreateKing(Vector3Int cell, Vector3 worldPosition)
    {
        if (!MapManager.Instance.SelectPlayerBase(cell))
            return false;

        GameObject king = MonoSingleton<ObjectPoolManager>.Instance.Pull(EUnitType.King);
        HpBar hpBar = MonoSingleton<ObjectPoolManager>.Instance.Pull(EUIPrefabType.UnitHpBar).GetComponent<HpBar>();
        king.transform.position = worldPosition;
        hpBar.SetPosition(king.transform.position);
        king.GetComponent<Unit>().HpBar = hpBar;

        MapManager.Instance.ConvertKingToBuildable(cell);
        MapManager.Instance.RegisterTower(cell, king);
        SimpleSingleton<MapUnitManager>.Instance.AddUnit(cell, king.GetComponent<Unit>());


        return true;
    }

    void ChangeToGameState()
    {
        MonoSingleton<GameStateManager>.Instance.ChangeState(EGameStateType.Game);
    }

    #region Interface
    void IState.Loop()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleMouseClick();
        }
    }

    void IState.Enter()
    {

    }

    void IState.Exit()
    {

    }
    #endregion
}