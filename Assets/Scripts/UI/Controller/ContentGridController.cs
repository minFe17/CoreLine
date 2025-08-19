using UnityEngine;
using UnityEngine.UI;

public class ContentGridController : MonoBehaviour
{
    private GridLayoutGroup _layoutGroup;
    private RectTransform _content;
    private void Awake()
    {
        _layoutGroup = GetComponent<GridLayoutGroup>();
    }
    private void Start()
    {
        _content = GameObject.Find("Scroll View").GetComponent<RectTransform>();
        SetRectSize();
    }
    private void SetRectSize()
    {
        float width = _content.rect.width;
        int columns = 4;
        int spacing = 15;

        float cellSize = (width - (spacing * columns)) / columns;

        _layoutGroup.cellSize = new Vector2(cellSize, cellSize);
    }
}
