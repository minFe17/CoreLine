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

        _layoutGroup.padding.left = _paddingLeft;
        _layoutGroup.padding.top = _paddingTop;
        float cellWidthSize = (width - (_widthSpacing * _columns)) / _columns;
        float cellHeightSize = (height - (_heightSpacing * _rows)) / _rows;

        _layoutGroup.cellSize = new Vector2(cellWidthSize, cellHeightSize);
        _layoutGroup.spacing = new Vector2(_widthSpacing, _heightSpacing);
    }
}
