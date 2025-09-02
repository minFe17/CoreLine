using UnityEngine;
public class Plain : MonoBehaviour, IMoveStyle
{
    public (Vector3, float) Tick(Vector3 curr, Vector3 baseT, Vector3 dir, float baseSpeed, float dt)
        => (baseT, 1f);
}
