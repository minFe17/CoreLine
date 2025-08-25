using UnityEngine;
using UnityEngine.UI;

public class ContentGridController : GridController
{
    private RectTransform _content;
    protected override void Awake()
    {
        base.Awake();
        _layoutGroup = GetComponent<GridLayoutGroup>();
    }
    protected override void Start()
    {
        _content = GameObject.Find("Scroll View").GetComponent<RectTransform>();
        SetRectSize();
    }
    protected override void SetRectSize()
    {
        float width = _content.rect.width;
        int spacing = 15;

        float cellSize = (width - (spacing * _columns)) / _columns;

        _layoutGroup.cellSize = new Vector2(cellSize, cellSize);
    }
}
