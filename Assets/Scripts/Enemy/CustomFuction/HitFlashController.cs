
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HitFlashTest : MonoBehaviour
{
    static readonly int ID_FlashAmount = Shader.PropertyToID("_HitFlash"); 
    static readonly int ID_FlashColor = Shader.PropertyToID("Tint");     

    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = Color.white; 
    [SerializeField] private float flashDuration = 0.1f;

    [SerializeField] private float scaleUpAmount = 0.1f;   
    [SerializeField] private float scaleReturnSpeed = 10f;

    [SerializeField] private Camera clickCamera;    
    [SerializeField] private LayerMask interactMask = ~0; 


    private SpriteRenderer sr;
    private MaterialPropertyBlock mpb;
    private float flashTimer;

    private Vector3 originalScale;
    private Vector3 targetScale;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (!sr)
        {
            enabled = false;
            return;
        }

        mpb = new MaterialPropertyBlock();
        ApplyFlash(0f);

        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        if (!enabled) return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TriggerHitEffect();
        }


        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            float amount = Mathf.Clamp01(flashTimer / flashDuration);
            ApplyFlash(amount);
        }

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleReturnSpeed);
    
    }


    private void ApplyFlash(float amount)
    {
        if (!sr) return;

        sr.GetPropertyBlock(mpb);
        mpb.SetFloat(ID_FlashAmount, amount);
        mpb.SetColor(ID_FlashColor, flashColor);
        sr.SetPropertyBlock(mpb);
    }

    public void TriggerHitEffect()
    {
        if (!enabled || !sr) return;

        flashTimer = flashDuration;
        ApplyFlash(1f);

        targetScale = originalScale * (1f + scaleUpAmount);

        CancelInvoke(nameof(ResetScale));
        Invoke(nameof(ResetScale), flashDuration * 0.5f); 
    }

    private void ResetScale()
    {
        targetScale = originalScale;
    }


    public void ClearFlash()
    {
        if (!enabled || !sr) return;

        flashTimer = 0f;
        ApplyFlash(0);
        ResetScale();
    }
}
