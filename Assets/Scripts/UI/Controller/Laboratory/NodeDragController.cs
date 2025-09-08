using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class NodeDragController : MonoBehaviour, IDragHandler
{
    private Dictionary<LaboratoryType, RectTransform> _contents = new();
    private RectTransform _currentRect;

    private void OnEnable()
    {
        EventManager.Instance.Subscribe<LaboratoryType>("ChoiceContent", SetContent);
    }
    private void OnDisable()
    {
        EventManager.Instance.UnSubscribe("SetContent",(Action<LaboratoryType>)SetContent);
    }
    private void Awake()
    {
        MatchContent();
    }
    private void MatchContent()
    {
        RectTransform[] list = GetComponentsInChildren<RectTransform>(includeInactive: true);
        foreach (RectTransform t in list)
        {
            if (t == this.transform) continue; // 자기 자신 제외

            // 이름이 enum 값과 같다고 가정
            if (Enum.TryParse<LaboratoryType>(t.name, out LaboratoryType type))
            {
                if (!_contents.ContainsKey(type))
                    _contents.Add(type, t);
            }
        }
    }
    public void OnDrag(PointerEventData eventData)
    {
        float minX = -1000f;
        float maxX = 0f;
        float minY = 500f;
        float maxY = 1500f;
        if (_currentRect == null) return;

        // 기존 위치에 델타 더함
        Vector2 newPos = _currentRect.anchoredPosition + eventData.delta;

        // x/y 범위 제한
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        _currentRect.anchoredPosition = newPos;

    }
    public void SetContent(LaboratoryType type)
    {
        _currentRect = _contents[type];
    }
}
