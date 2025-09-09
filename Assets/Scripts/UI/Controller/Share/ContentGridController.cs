using UnityEngine;
using UnityEngine.UI;

public class ContentGridController : GridController
{
    [SerializeField]
    private string _parentPath = "Scroll View";
    private RectTransform _content;
    protected override void Awake()
    {
        base.Awake();
        _layoutGroup = GetComponent<GridLayoutGroup>();
    }
    protected override void Start()
    {
        _content = GameObject.Find(_parentPath).GetComponent<RectTransform>();
        
        SetRectSize();
    }
    protected override void SetRectSize()
    {
        if (_content == null) return;

        float width = _content.rect.width;

        // 총 padding 및 spacing 계산
        float totalHorizontalPadding = _paddingLeft + _paddingRight;
        float totalHorizontalSpacing = _widthSpacing * (_columns - 1);

        // 가용 너비
        float availableWidth = width - totalHorizontalPadding - totalHorizontalSpacing;

        float cellSize = availableWidth / _columns;

        // 패딩 및 spacing 설정
        _layoutGroup.padding.left = _paddingLeft;
        _layoutGroup.padding.right = _paddingRight;
        _layoutGroup.padding.top = _paddingTop;
        _layoutGroup.padding.bottom = _paddingBottom;

        _layoutGroup.spacing = new Vector2(_widthSpacing, _heightSpacing);
        _layoutGroup.cellSize = new Vector2(cellSize, cellSize); // 정사각형 셀
    }
}

