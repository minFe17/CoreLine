using UnityEngine;
public class Charge : MonoBehaviour, IMoveStyle
{
    [SerializeField] private float _accel = 3f, _maxMul = 1.8f;
    [SerializeField] private float _dashMul = 2.6f, _dashDur = 0.25f, _dashCD = 2.0f;
    private float v = 1f, dashT = 0f, cd = 0f;

    public (Vector3, float) Tick(Vector3 curr, Vector3 baseT, Vector3 dir, float baseSpeed, float dt)
    {
        if (dashT > 0f) { dashT -= dt; return (baseT, _dashMul); }
        cd -= dt; v = Mathf.MoveTowards(v, _maxMul, _accel * dt);
        if (cd <= 0f) { dashT = _dashDur; cd = _dashCD; }
        return (baseT, v);
    }
}
