using UnityEngine;
using Utils;

public class ChainThunder : MonoBehaviour
{
    LineRenderer _lineRenderer;
    float _lifeTime = 1f;

    public void Init(Vector3 start, Vector3 end)
    {
        if(_lineRenderer == null )
            _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.SetPosition(0, start);
        _lineRenderer.SetPosition(1,end);

        Invoke("Remove", _lifeTime);
    }

    void Remove()
    {
        MonoSingleton<ObjectPoolManager>.Instance.Push(EBulletType.ChainThunder, gameObject);
    }
}