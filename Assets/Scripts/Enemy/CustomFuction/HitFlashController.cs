
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
        mpb = new MaterialPropertyBlock();
        ApplyFlash(0f);

        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TriggerHitEffect();
        }

        if (Input.GetMouseButtonDown(0))
        {
            var cam = clickCamera != null ? clickCamera : Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);

            RaycastHit2D hit = Physics2D.GetRayIntersection(ray, Mathf.Infinity, interactMask);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                TriggerHitEffect(); 
            }
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
        sr.GetPropertyBlock(mpb);
        mpb.SetFloat(ID_FlashAmount, amount);
        mpb.SetColor(ID_FlashColor, flashColor);
        sr.SetPropertyBlock(mpb);
    }

    public void TriggerHitEffect()
    {
        flashTimer = flashDuration;
        ApplyFlash(1f);

        targetScale = originalScale * (1f + scaleUpAmount);

        CancelInvoke(nameof(ResetScale));
        Invoke(nameof(ResetScale), flashDuration * 0.5f); 
    }

    void ResetScale()
    {
        targetScale = originalScale;
    }
}
