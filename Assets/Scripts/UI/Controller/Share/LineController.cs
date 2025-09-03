using UnityEngine;

public class LineController : MonoBehaviour
{
    private float _lineWidth = 20.0f;
    private bool _isChangeColor = false;
    private RectTransform _rectTransform;

    private RectTransform _startRect;
    private RectTransform _endRect;

    private LaboratoryNode _startNode;
    private LaboratoryNode _endNode;

    private UIColorApplier _color;


    public void SetTargets(LaboratoryNode start, LaboratoryNode end)
    {
        _startRect = start.GetComponent<RectTransform>();
        _endRect = end.GetComponent<RectTransform>(); 

        _startNode = start;
        _endNode = end;

        UpdateLine(); // 초기 그리기
    }

    public void UpdateLine()
    {
        if (_startRect == null || _endRect == null)
            return;

        Vector2 start = _startRect.anchoredPosition;
        Vector2 end = _endRect.anchoredPosition;

        Vector2 direction = end - start;

        float length = direction.magnitude;

        _rectTransform.sizeDelta = new Vector2(length, _lineWidth);
        _rectTransform.pivot = new Vector2(0, 0.5f); // 왼쪽 기준 회전
        _rectTransform.anchoredPosition = start;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
    }
    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _color = GetComponent<UIColorApplier>();
    }
    private void Update()
    {
       if (_isChangeColor) return;
       if(_startNode.IsUnlocked&&_endNode.IsUnlocked)
       {
           _isChangeColor = true;
           _color.MyColorType = ColorType.Normal;
       }

    }
}

