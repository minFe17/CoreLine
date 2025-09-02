using UnityEngine;
public class Hop : MonoBehaviour, IMoveStyle
{
    [SerializeField] private float _hopFreq = 7f;
    [SerializeField] private float _hopGain = 0.30f;
    private float _phase;

    private void OnEnable() { _phase = Random.value * Mathf.PI * 2f; }

    public (Vector3, float) Tick(Vector3 curr, Vector3 baseT, Vector3 dir, float baseSpeed, float dt)
    {
        _phase += (_hopFreq + baseSpeed) * dt;
        float s = Mathf.Sin(_phase);
        float mul = 1f + Mathf.Sign(s) * _hopGain * (s * s); 
        return (baseT, mul);
    }
}
