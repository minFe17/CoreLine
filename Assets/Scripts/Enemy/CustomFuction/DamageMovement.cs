using UnityEngine;
using System.Collections;

public class DamageFlash : MonoBehaviour
{
    public float flashDuration = 0.5f;
    public float flashInterval = 0.1f;
    private Renderer objectRenderer;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();   
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartFlashing();
        }
    }

    public void StartFlashing()
    {
        StartCoroutine(FlashEffect());
    }

    IEnumerator FlashEffect()
    {
        float timer = 0f;
        while (timer < flashDuration)
        {
            objectRenderer.enabled = false;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;

            objectRenderer.enabled = true;
            yield return new WaitForSeconds(flashInterval);
            timer += flashInterval;
        }

        objectRenderer.enabled = true;

    }
}
