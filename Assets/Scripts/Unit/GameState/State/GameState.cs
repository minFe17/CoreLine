using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class GameState : IState, IMediatorEvent
{
    BuildUI _buildUI;
    Canvas _canvas;

    bool _isSelectUnit;
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
        CreateUI();
        //CheckDestory();
    }

    void CreateUI()
    {
        MapManager.PlaceInfo info = MapManager.Instance.GetPlaceInfo(_cell);
        if (!info.Placeable || info.Occupied)
            return;
        _isSelectUnit = true;
        if (_buildUI == null)
        {
            if(_canvas == null)
            {
                var temp = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                _canvas = temp.GetComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
            _buildUI = UnityEngine.Object.Instantiate(SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.UI).GetPrefab(EUIPrefabType.BuildUI)).GetComponent<BuildUI>();
        }
        _buildUI.OpenAtCell(_cell, _test);
    }

    public void CreateUnit(EUnitType unitType)
    {
        GameObject unit = MonoSingleton<ObjectPoolManager>.Instance.Pull(unitType);
        unit.transform.position = _worldPosition;

        MapManager.Instance.RegisterTower(_cell, unit);
        unit.GetComponent<TowerUnit>().Cell = _cell;
        _buildUI.Close();
    }

    //public void CheckDestory()
    //{
    //    if(MapManager.Instance.IsDestructible(_cell))
    //}

    #region Interface
    void IState.Loop()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            if (_isSelectUnit)
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
        _isSelectUnit = false;
    }
    #endregion
}