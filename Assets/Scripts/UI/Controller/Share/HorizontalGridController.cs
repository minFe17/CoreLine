using UnityEngine;
using UnityEngine.UI;

public class HorizontalGridController : GridController
{
    protected HorizontalLayoutGroup _horizontalLayoutGroup;
    protected override void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
    }

    protected override void SetRectSize()
    {
        float width = _rectTransform.rect.width;
        float height = _rectTransform.rect.height;

        // padding, spacing 설정
        _horizontalLayoutGroup.padding.left = _paddingLeft;
        _horizontalLayoutGroup.padding.right = _paddingRight;
        _horizontalLayoutGroup.padding.top = _paddingTop;
        _horizontalLayoutGroup.padding.bottom = _paddingBottom;
        _horizontalLayoutGroup.spacing = _widthSpacing;

        int count = _columns; 
        if (count <= 0) return;

        float totalSpacing = _widthSpacing * (count - 1);
        float totalPadding = _paddingLeft + _paddingRight;
        float availableWidth = width - totalSpacing - totalPadding;

        float cellWidth = availableWidth / count;
        float cellHeight = height - _paddingTop - _paddingBottom;

        // 실존 자식 수
        int childCount = _rectTransform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            RectTransform child = _rectTransform.GetChild(i) as RectTransform;
            if (child == null) continue;

            child.sizeDelta = new Vector2(cellWidth, cellHeight);
        }
    }

}
