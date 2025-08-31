using UnityEngine;

public interface IMoveStyle
{
    (Vector3 targetWorld, float speedMul) Tick(
        Vector3 currPos, Vector3 baseTarget, Vector3 dirNorm, float baseSpeed, float dt);

}