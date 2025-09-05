using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class StarUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image baseStar;   // BaseStar (은색). 사용 안 해도 됨.
    [SerializeField] private Image fillStar;   // FillStar (노란색, Image.type = Filled)

    [Header("Anim")]
    [SerializeField] private float fillDuration = 0.5f; // 0 → 1 또는 1 → 0
    [SerializeField] private bool useUnscaledTime = true;

    private Coroutine co;

    private void Awake()
    {
        // 비어 있으면 자식에서 자동 할당 (이름 기준)
        if (baseStar == null || fillStar == null)
        {
            foreach (var img in GetComponentsInChildren<Image>(true))
            {
                if (img.name.Contains("Base")) baseStar = baseStar ?? img;
                else if (img.name.Contains("Fill")) fillStar = fillStar ?? img;
            }
        }
        // 백업: 자식 이미지 2개 이상이면 순서로 채움
        if (baseStar == null || fillStar == null)
        {
            var imgs = GetComponentsInChildren<Image>(true);
            if (imgs.Length >= 2)
            {
                baseStar = baseStar ?? imgs[0];
                fillStar = fillStar ?? imgs[1];
            }
        }
    }

    /// <summary>초기 상태로: 채움 0 (은색만 보임)</summary>
    public void InitIdle()
    {
        if (fillStar) fillStar.fillAmount = 0f;
    }

    /// <summary>
    /// 조건 하나의 연출.
    /// success=true → fillAmount를 0→1로
    /// success=false → fillAmount를 1→0 또는 0 유지
    /// </summary>
    public void PlayCondition(bool success)
    {
        if (co != null) StopCoroutine(co);
        float target = success ? 1f : 0f;
        co = StartCoroutine(CoFillTo(target));
    }

    /// <summary>애니메이션 없이 즉시 결과만 반영</summary>
    public void SetResult(bool success)
    {
        if (co != null) StopCoroutine(co);
        if (fillStar) fillStar.fillAmount = success ? 1f : 0f;
    }

    private IEnumerator CoFillTo(float target)
    {
        if (fillStar == null) yield break;

        float start = fillStar.fillAmount;
        if (Mathf.Approximately(start, target))
        {
            fillStar.fillAmount = target;
            yield break;
        }

        float t = 0f;
        float dur = Mathf.Max(0.0001f, fillDuration);
        while (t < dur)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float a = Mathf.Clamp01(t / dur);
            fillStar.fillAmount = Mathf.Lerp(start, target, a);
            yield return null;
        }
        fillStar.fillAmount = target;
        co = null;
    }
}
