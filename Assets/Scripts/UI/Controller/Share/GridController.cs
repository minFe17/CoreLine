using UnityEngine;
using UnityEngine.UI;

public class GridController : MonoBehaviour
{
    [SerializeField]
    protected int _columns = 4;
    [SerializeField]
    protected int _rows = 2;
    [SerializeField]
    protected int _paddingLeft = 30;
    [SerializeField]
    protected int _paddingTop = 20;
    [SerializeField]
    protected int _paddingRight = 30;
    [SerializeField]
    protected int _paddingBottom = 20;
    [SerializeField]
    protected int _widthSpacing = 20;
    [SerializeField]
    protected int _heightSpacing = 20;

    protected GridLayoutGroup _layoutGroup;
    protected RectTransform _rectTransform;
    protected virtual void Awake()
    {
        _layoutGroup = GetComponent<GridLayoutGroup>();
        _rectTransform = GetComponent<RectTransform>();
    }
    protected virtual void Start()
    {
        SetRectSize();
    }
    protected virtual void SetRectSize()
    {
        float width = _rectTransform.rect.width;
        float height = _rectTransform.rect.height;

        // 패딩 설정
        _layoutGroup.padding.left = _paddingLeft;
        _layoutGroup.padding.top = _paddingTop;
        _layoutGroup.padding.right = _paddingRight;
        _layoutGroup.padding.bottom = _paddingBottom;

        // cell 크기 계산 (패딩 + spacing 포함)
        float totalHorizontalPadding = _paddingLeft + _paddingRight;
        float totalVerticalPadding = _paddingTop + _paddingBottom;

        float totalHorizontalSpacing = _widthSpacing * (_columns - 1);
        float totalVerticalSpacing = _heightSpacing * (_rows - 1);

        float availableWidth = width - totalHorizontalPadding - totalHorizontalSpacing;
        float availableHeight = height - totalVerticalPadding - totalVerticalSpacing;

        float cellWidthSize = availableWidth / _columns;
        float cellHeightSize = availableHeight / _rows;

        _layoutGroup.cellSize = new Vector2(cellWidthSize, cellHeightSize);
        _layoutGroup.spacing = new Vector2(_widthSpacing, _heightSpacing);
    }
}
