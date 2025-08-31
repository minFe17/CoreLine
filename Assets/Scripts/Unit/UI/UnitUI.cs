using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Utils;

public class UnitUI : MonoBehaviour, IMediatorEvent
{
    [SerializeField] List<RectTransform> _buttonPosition;

    RectTransform _rectTransform;
    float _radius = 100;
    bool _isChangePosition;

    void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _rectTransform.offsetMin = Vector3.zero;
        _rectTransform.offsetMax = Vector3.zero;
        SimpleSingleton<MediatorManager>.Instance.Register(EMediatorType.OpenUnitUI, this);
        CalculateButtonPosition();
        Close();
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

        gameObject.SetActive(true);
        Vector3 pos = MapManager.Instance.CellCenterWorld(cell);
        Camera uiCam = null;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, pos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform.parent as RectTransform, screenPoint, uiCam, out Vector2 localPoint);
        _rectTransform.anchoredPosition = localPoint;
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

    }

    public void Sell()
    {

    }
    #endregion

    #region Interface
    void IMediatorEvent.HandleEvent(object data)
    {
        Vector3Int cell = (Vector3Int)data;
        Open(cell);
    }
    #endregion
}