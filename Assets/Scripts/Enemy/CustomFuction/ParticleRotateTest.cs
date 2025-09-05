using UnityEngine;

public class ParticleRotateTest : MonoBehaviour
{
    [SerializeField] private ParticleSystem targetParticle; // 회전시킬 파티클 시스템

    private void Update()
    {
        // Space 키를 누르면 Y축 기준으로 90도 회전
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (targetParticle != null)
            {
                // 현재 회전값에 90도 추가
                targetParticle.transform.rotation *= Quaternion.Euler(90f, 0f, 0f);
                Debug.Log("파티클 회전됨: " + targetParticle.transform.rotation.eulerAngles);
            }
        }
    }
}
