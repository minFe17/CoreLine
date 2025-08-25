using UnityEngine;
using UnityEngine.UI;

public class GridController : MonoBehaviour
{
    [SerializeField]
    protected int _columns = 4;
    [SerializeField]
    protected int _rows = 2;

    protected int _paddingLeft = 30;
    protected int _paddingTop = 20;

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
        int spacing = 20;

        _layoutGroup.padding.left = _paddingLeft;
        _layoutGroup.padding.top = _paddingTop;
        float cellWidthSize = (width - (spacing * _columns)) / _columns;
        float cellHeightSize = (height - (spacing * _rows)) / _rows;

        _layoutGroup.cellSize = new Vector2(cellWidthSize, cellHeightSize);
    }
}
