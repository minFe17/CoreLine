using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class GameState : IState, IMediatorEvent
{
    BuildUI _buildUI;
    TwoButtonUI _twoButtonUI;
    Canvas _canvas;

    bool _isSelectTile;
    Vector3Int _cell;
    Vector3 _worldPosition;

    List<EUnitType> _test = new List<EUnitType>() { EUnitType.Archer, EUnitType.Warrior, EUnitType.Wizard, EUnitType.Assassin, EUnitType.Chef, EUnitType.Pirate };

    public GameState()
    {
        SimpleSingleton<MediatorManager>.Instance.Register(EMediatorType.EndSelectUnit, this);
        
    }

    void HandleMouseClick()
    {
        Vector3 mouseWorld = SimpleSingleton<UnitPlacementManager>.Instance.GetMouseWorldPosition();
        Vector3Int cell = SimpleSingleton<UnitPlacementManager>.Instance.GetCellFromWorld(mouseWorld);
        Vector3 cellCenter = MapManager.Instance.GetCellCenterWorld(cell);

        _cell = cell;
        _worldPosition = cellCenter;

        ShowBuildUI();
        ShowDestructUI();
    }

    void CreateUI()
    {
        if (_canvas == null)
        {
            GameObject temp = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvas = temp.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        _buildUI = UnityEngine.Object.Instantiate(SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.UI).GetPrefab(EUIPrefabType.BuildUI)).GetComponent<BuildUI>();
        _twoButtonUI = UnityEngine.Object.Instantiate(SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.UI).GetPrefab(EUIPrefabType.TwoButtonPanel)).GetComponent<TwoButtonUI>();
    }

    void ShowBuildUI()
    {
        MapManager.PlaceInfo info = MapManager.Instance.GetPlaceInfo(_cell);
        if (info.Occupied)
        {

        }
        if (!info.Placeable)
            return;
        if (_canvas == null)
            CreateUI();
        _isSelectTile = true;
        _buildUI.OpenAtCell(_cell, _test);
    }

    void ShowDestructUI()
    {
        //if (!MapManager.Instance.IsDestructible(_cell))
        //    return;
        //if (_canvas == null)
        //    CreateUI();
        //_isSelectTile = true;
        //_destructUI.OpenAtCell(_cell);
    }

    public void CreateUnit(EUnitType unitType)
    {
        GameObject unit = MonoSingleton<ObjectPoolManager>.Instance.Pull(unitType);
        unit.transform.position = _worldPosition;

        MapManager.Instance.RegisterTower(_cell, unit);
        SimpleSingleton<MapUnitManager>.Instance.AddUnit(_cell, unit.GetComponent<Unit>());
        unit.GetComponent<TowerUnit>().Cell = _cell;
        _buildUI.Close();
    }

    #region Interface
    void IState.Loop()
    {

        if (Input.GetMouseButtonDown(0))
        {
            if (_isSelectTile)
                return;
            HandleMouseClick();
        }
    }

    void IState.Enter()
    {

    }

    void IState.Exit()
    {

    }

    void IMediatorEvent.HandleEvent(object data)
    {
        _isSelectTile = false;
    }
    #endregion
}