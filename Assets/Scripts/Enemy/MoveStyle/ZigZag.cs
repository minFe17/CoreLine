using UnityEngine;
public class ZigZag : MonoBehaviour, IMoveStyle
{
    [SerializeField] private float _amplitudeCells = 0.30f; 
    [SerializeField] private float _wavelength = 2.0f;      
    [SerializeField] private float _cornerFade = 0.35f;
    private float _seed;

    private void OnEnable() { _seed = Random.value * 1000f; }

    public (Vector3, float) Tick(Vector3 curr, Vector3 baseT, Vector3 dir, float baseSpeed, float dt)
    {
        if (dir.sqrMagnitude < 1e-6f) return (baseT, 1f);

        Vector3 n = new(-dir.y, dir.x, 0f); 
        float dist = Vector3.Distance(curr, baseT);
        float fade = Mathf.Clamp01((dist - _cornerFade) / Mathf.Max(0.001f, _cornerFade));

        float k = (2f * Mathf.PI) / Mathf.Max(0.01f, _wavelength);
        float s = Mathf.Sin((Time.time + _seed) * k);
        float amp = _amplitudeCells * FindAnyObjectByType<TestMap>().CellSize * fade;

        return (baseT + n * (amp * s), 1f);
    }
}
