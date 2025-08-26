using UnityEngine;
using System.Collections; // 코루틴 사용을 위해

public class DamageFlash : MonoBehaviour
{
    public float flashDuration = 0.5f; // 깜빡임 지속 시간
    public float flashInterval = 0.1f; // 깜빡이는 간격
    private Renderer objectRenderer;
    private Color originalColor; // 원래 색상 저장 (깜빡임 후 원래 색으로 되돌릴 경우)

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color; 
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            StartFlashing();
        }
    }

    // 데미지를 받았을 때 호출하는 함수
    public void StartFlashing()
    {
        StartCoroutine(FlashEffect());
    }

    IEnumerator FlashEffect()
    {
        float timer = 0f;
        while (timer < flashDuration)
        {
            // 오브젝트를 끄고
            objectRenderer.enabled = false;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;

            // 오브젝트를 켜고
            objectRenderer.enabled = true;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }
        
        objectRenderer.enabled = true;
        
    }
}