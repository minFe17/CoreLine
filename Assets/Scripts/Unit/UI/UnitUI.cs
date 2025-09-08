using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;
using Utils;

public class UnitUI : MonoBehaviour, IMediatorEvent
{
    [SerializeField] List<RectTransform> _buttonPosition;
    [SerializeField] Text _upgradeCostText;

    Unit _unit;
    SpriteAtlas _atlas;
    RectTransform _rectTransform;
    float _radius = 150;
    bool _isChangePosition;

    public Unit Unit { get => _unit; }

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        SimpleSingleton<MediatorManager>.Instance.Register(EMediatorType.OpenUnitUI, this);
        CalculateButtonPosition();
        Close();
        _atlas = SimpleSingleton<PrefabManager>.Instance.GetPrefabLoad(EPrefabType.SpriteAtlas).GetPrefabAtlas(EAtlasPrefabType.UnitUIIcon);
        _buttonPosition[1].GetComponent<Image>().sprite = _atlas.GetSprite(EUnitUIIconType.Sell.ToString());
    }

    void CalculateButtonPosition()
    {
        int count = _buttonPosition.Count;
        for (int i = 0; i < count; i++)
        {
            RectTransform rect = _buttonPosition[i];

            // 기준 통일
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            // 반시계 방향으로 회전 (0번을 위로)
            float angle = -((360f / count) * i) + 90f;
            float rad = angle * Mathf.Deg2Rad;

            float x = Mathf.Cos(rad) * _radius;
            float y = Mathf.Sin(rad) * _radius;

            rect.anchoredPosition = new Vector2(x, y);
        }
    }

    void Open(Vector3Int cell)
    {
        if (gameObject.activeSelf)
            _isChangePosition = true;
        else
            _isChangePosition = false;

        SetButton();

        gameObject.SetActive(true);
        Vector3 pos = MapManager.Instance.CellCenterWorld(cell);
        Camera uiCam = null;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, pos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform.parent as RectTransform, screenPoint, uiCam, out Vector2 localPoint);
        _rectTransform.anchoredPosition = localPoint;
    }

    void SetButton()
    {
        if (_unit is TowerUnit unit)
        {
            if (unit.IsMaxLevel())
            {
                _buttonPosition[0].GetComponent<Image>().sprite = _atlas.GetSprite(EUnitUIIconType.Fusion.ToString());
                _upgradeCostText.text = SimpleSingleton<FusionManager>.Instance.Cost.ToString();
            }
            else
            {
                _buttonPosition[0].GetComponent<Image>().sprite = _atlas.GetSprite(EUnitUIIconType.Upgrade.ToString());
                _upgradeCostText.text = SimpleSingleton<UnitDataList>.Instance.GetUnitData(unit.UnitType).LevelData[unit.Level + 1].Cost.ToString();
            }
        }
    }

    public bool IsClickOnBlockButton()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (RaycastResult result in raycastResults)
        {
            RectTransform rt = result.gameObject.GetComponent<RectTransform>();
            if (rt != null && _buttonPosition.Contains(rt))
                return true;
        }
        return false;
    }

    #region Button Click Event
    public void Close()
    {
        if (_isChangePosition)
        {
            _isChangePosition = false;
            return;
        }
        gameObject.SetActive(false);
    }

    public void UpgradeOrFusion()
    {
        SimpleSingleton<AttackRangeManager>.Instance.HideAttackRange();
        if (_unit is TowerUnit unit)
        {
            if (unit.IsMaxLevel())
            {
                if (CostManager.Instance.TrySpend(CostManager.CostType.Unit, SimpleSingleton<FusionManager>.Instance.Cost))
                    unit.Fusion();
            }
            else
                unit.Upgrade();
            Close();
        }
    }

    public void Sell()
    {
        SimpleSingleton<AttackRangeManager>.Instance.HideAttackRange();

        int value = 0;
        if (_unit is TowerUnit unit)
            value = unit.UnitTotalCost / 2;
        else if (_unit is FusionUnit fusionUnit)
            value = SimpleSingleton<FusionManager>.Instance.Cost / 5;

        CostManager.Instance.Add(CostManager.CostType.Unit, value);
        _unit.Remove();
        Close();
    }
    #endregion

    #region Interface
    void IMediatorEvent.HandleEvent(object data)
    {
        _unit = (Unit)data;
        _unit.UnitUI = this;
        Open(_unit.Cell);
    }
    #endregion
}