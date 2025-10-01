using UnityEngine;
using UnityEngine.EventSystems;

public class TouchEffect : MonoBehaviour
{
    [SerializeField] GameObject _touchEffectPrefab;
    [SerializeField] Canvas _canvas;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                SpawnEffect(Input.mousePosition);
            }
        }
    }

    void SpawnEffect(Vector2 screenPosition)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            screenPosition,
            _canvas.worldCamera,
            out Vector2 localPoint
        );

        GameObject effect = Instantiate(_touchEffectPrefab, _canvas.transform);
        effect.GetComponent<RectTransform>().anchoredPosition = localPoint;

        Destroy(effect, 0.3f); 
    }
}