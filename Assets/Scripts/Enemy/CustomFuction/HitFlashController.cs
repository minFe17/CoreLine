//using UnityEngine;
//using System.Collections; 

//public class DamageFlash : MonoBehaviour
//{
//    public float flashDuration = 0.5f; 
//    public float flashInterval = 0.1f; 
//    private Renderer objectRenderer;
//    private Color originalColor; 

//    void Start()
//    {
//        objectRenderer = GetComponent<Renderer>();
//        if (objectRenderer != null)
//        {
//            originalColor = objectRenderer.material.color; 
//        }
//    }

//    private void Update()
//    {
//        if(Input.GetKeyDown(KeyCode.Alpha1))
//        {
//            StartFlashing();
//        }
//    }

//    public void StartFlashing()
//    {
//        StartCoroutine(FlashEffect());
//    }

//    IEnumerator FlashEffect()
//    {
//        float timer = 0f;
//        while (timer < flashDuration)
//        {
//            objectRenderer.enabled = false;
//            yield return new WaitForSeconds(flashInterval);
//            timer += flashInterval;

//            objectRenderer.enabled = true;
//            yield return new WaitForSeconds(flashInterval);
//            timer += flashInterval;
//        }

//        objectRenderer.enabled = true;

//    }
//}

using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HitFlashTest : MonoBehaviour
{
    static readonly int ID_FlashAmount = Shader.PropertyToID("_HitFlash"); 
    static readonly int ID_FlashColor = Shader.PropertyToID("Tint");     

    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = Color.white; 
    [SerializeField] private float flashDuration = 0.1f;

    private SpriteRenderer sr;
    private MaterialPropertyBlock mpb;
    private float flashTimer;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();
        ApplyFlash(0f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            flashTimer = flashDuration;
            ApplyFlash(1f);
        }

        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            float amount = Mathf.Clamp01(flashTimer / flashDuration);
            ApplyFlash(amount);
        }
    }

    void ApplyFlash(float amount)
    {
        sr.GetPropertyBlock(mpb);
        mpb.SetFloat(ID_FlashAmount, amount);
        mpb.SetColor(ID_FlashColor, flashColor);
        sr.SetPropertyBlock(mpb);
    }
}
