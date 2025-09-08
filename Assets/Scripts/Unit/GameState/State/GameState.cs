using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class GameState : IState, IMediatorEvent
{
    BuildUI _buildUI;
    TwoButtonUI _twoButtonUI;
    UnitUI _unitUI;
    Canvas _canvas;

    bool _isSelectTile;
    Vector3Int _cell;
    Vector3 _worldPosition;

    List<EUnitType> _test = new List<EUnitType>() { EUnitType.Hammer, EUnitType.ThunderWizard, EUnitType.Gunner, EUnitType.Archer, EUnitType.Assassin, EUnitType.Wizard };

    public GameState()
    {
        SimpleSingleton<MediatorManager>.Instance.Register(EMediatorType.EndSelectTile, this);
        CreateUI();
    }

    void HandleMouseClick()
    {
        SimpleSingleton<AttackRangeManager>.Instance.HideAttackRange();
        _unitUI.Close();
        Vector3 mouseWorld = SimpleSingleton<UnitPlacementManager>.Instance.GetMouseWorldPosition();
        Vector3Int cell = SimpleSingleton<UnitPlacementManager>.Instance.GetCellFromWorld(mouseWorld);
        Vector3 cellCenter = MapManager.Instance.GetCellCenterWorld(cell);

        _cell = cell;
        _worldPosition = cellCenter;

        ShowBuildUI();
        ShowMapInteractUI();
    }

    void CreateUI()
    {
        if (_canvas == null)
        {
            GameObject temp = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            _canvas = temp.GetComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        _buildUI = Object.Instantiate(SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.UI).GetPrefab(EUIPrefabType.BuildUI)).GetComponent<BuildUI>();
        _twoButtonUI = Object.Instantiate(SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.UI).GetPrefab(EUIPrefabType.TwoButtonPanel)).GetComponent<TwoButtonUI>();
        _twoButtonUI.transform.SetParent(_canvas.transform);
        _unitUI = Object.Instantiate(SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.UI).GetPrefab(EUIPrefabType.UnitUI)).GetComponent<UnitUI>();
        _unitUI.transform.SetParent(_canvas.transform);
    }

    void ShowBuildUI()
    {
        MapManager.PlaceInfo info = MapManager.Instance.GetPlaceInfo(_cell);

        if (info.Occupied)
        {
            Unit unit = SimpleSingleton<MapUnitManager>.Instance.GetUnit(_cell);
            if (unit != null)
                unit.ClickUnit();
        }
        if (!info.Placeable)
            return;
        if (_canvas == null)
            CreateUI();
        _isSelectTile = true;
        _unitUI.Close();
        _buildUI.OpenAtCell(_cell, _test);
    }

    void ShowMapInteractUI()
    {
        if (TryOpenObjectTilePanel()) return;
        if (MapManager.Instance.IsDestructible(_cell))
        {
            _isSelectTile = true;
            _unitUI.Close();
            _twoButtonUI.OpenAtCell(_cell, "파괴", (id, payload) =>
            {
                MapManager.Instance.DestroyWallAt((Vector3Int)payload);
            });
        }
    }

    bool TryOpenObjectTilePanel()
    {
        Camera camera = Camera.main;

        Vector3 wp = camera.ScreenToWorldPoint(Input.mousePosition);
        wp.z = 0f;

        // 겹침 고려해서 전 레이어 검사
        Collider2D[] hits = Physics2D.OverlapPointAll((Vector2)wp, ~0);
        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D h = hits[i];
            if (h && h.TryGetComponent(out ObjectTile objectTile))
            {
                _isSelectTile = true;
                _unitUI.Close();
                _twoButtonUI.OpenAtObject(objectTile, "발동", (id, payload) =>
                {
                    if (payload is ObjectTile objectTile) 
                        objectTile.Activate();
                });
                return true;
            }
        }
        return false;
    }

    public void CreateUnit(EUnitType unitType)
    {
        int cost = SimpleSingleton<UnitDataList>.Instance.GetUnitData(unitType).LevelData[0].Cost;
        if (!CostManager.Instance.TrySpend(CostManager.CostType.Unit, cost))
            return;
        GameObject unit = MonoSingleton<ObjectPoolManager>.Instance.Pull(unitType);
        HpBar hpBar = MonoSingleton<ObjectPoolManager>.Instance.Pull(EUIPrefabType.UnitHpBar).GetComponent<HpBar>();
        unit.transform.position = _worldPosition;
        hpBar.SetPosition(unit.transform.position);
        unit.GetComponent<Unit>().HpBar = hpBar;

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
            if (_unitUI.IsClickOnBlockButton())
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