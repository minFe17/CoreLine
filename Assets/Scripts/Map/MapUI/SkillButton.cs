using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillButton : MonoBehaviour
{
    [Header("Auto/Manual Index")]
    [SerializeField] private bool _autoIndexByOrder = true;
    [SerializeField] private int _slotIndex = 0;

    [Header("Targeting")]
    [SerializeField] private SkillTargetingController _targetingController;

    private Image _icon;                 // 버튼 아이콘(Image)
    private Image _cooldownOverlay;      // 쿨다운 반투명 원
    private TextMeshProUGUI _cdText;     // 남은 시간 표시
    private int _resolvedIndex = -1;

    // 프로시저 원형 스프라이트 캐시
    private static Sprite _circleSpriteCache;

    private void Awake()
    {
        if (_targetingController == null)
            _targetingController = FindFirstObjectByType<SkillTargetingController>(FindObjectsInactive.Include);

        _icon = GetComponent<Image>();
        EnsureCooldownVisuals();

        if (SkillManager.Instance != null)
            SkillManager.Instance.OnLoadoutChanged += RefreshIcon;
    }

    private void OnEnable()
    {
        RefreshIndex();
        RefreshIcon();
        StartCoroutine(CoRefreshNextFrame());
    }

    private IEnumerator CoRefreshNextFrame()
    {
        yield return null;
        RefreshIndex();
        RefreshIcon();
    }

    private void Update()
    {
        // 쿨타임 진행 표시
        if (SkillManager.Instance == null || _cooldownOverlay == null) return;

        if (_resolvedIndex >= 0 && _resolvedIndex < SkillManager.Instance._loadout.Count &&
            SkillManager.Instance.TryGetCooldownInfo(_resolvedIndex, out var remain, out var total, out var ratio))
        {
            bool onCd = remain > 0.001f;
            _cooldownOverlay.enabled = onCd;
            _cooldownOverlay.fillAmount = ratio;          // 1 → 0 로 줄어드는 형태

            if (_cdText != null)
            {
                if (onCd)
                {
                    // 0.0이 너무 일찍 뜨지 않게 0.1 단위 올림
                    float show = Mathf.Ceil(remain * 10f) / 10f;
                    if (show < 0.1f) show = 0.1f; // 시각적으로 0.0 직전까지 유지하고 싶을 때

                    _cdText.text = $"{show:0.0}"; // 항상 소수 1자리
                    _cdText.enabled = true;
                }
                else
                {
                    _cdText.enabled = false;
                }
            }
        }
        else
        {
            _cooldownOverlay.enabled = false;
            if (_cdText != null) _cdText.enabled = false;
        }
    }

    // ───────── 인덱스/아이콘 ─────────
    private void RefreshIndex()
    {
        if (!_autoIndexByOrder) { _resolvedIndex = _slotIndex; return; }

        Transform p = transform.parent;
        _resolvedIndex = _slotIndex;
        if (p == null) return;

        var buttons = new System.Collections.Generic.List<SkillButton>(p.childCount);
        for (int i = 0; i < p.childCount; i++)
        {
            var b = p.GetChild(i).GetComponent<SkillButton>();
            if (b != null && b.isActiveAndEnabled) buttons.Add(b);
        }

        if (buttons.Count == 0) return;

        buttons.Sort((a, b) =>
        {
            var ra = a.transform as RectTransform;
            var rb = b.transform as RectTransform;
            float ax = ra ? ra.anchoredPosition.x : a.transform.localPosition.x;
            float bx = rb ? rb.anchoredPosition.x : b.transform.localPosition.x;
            return ax.CompareTo(bx);
        });

        _resolvedIndex = buttons.IndexOf(this);
    }

    private void RefreshIcon()
    {
        if (_icon == null || SkillManager.Instance == null) return;

        if (_resolvedIndex < 0 || _resolvedIndex >= SkillManager.Instance._loadout.Count)
        { _icon.enabled = false; return; }

        var skill = SkillManager.Instance.GetSelectedSkillBySlotIndex(_resolvedIndex);
        Sprite s = Resources.Load<Sprite>($"Skills/{skill.Id}");
        _icon.sprite = s;
        _icon.enabled = (s != null);
    }

    // ───────── 비주얼 생성: 원형 오버레이 + 텍스트 ─────────
    private void EnsureCooldownVisuals()
    {
        // 오버레이
        var overlayTf = transform.Find("Cooldown");
        if (overlayTf != null)
            _cooldownOverlay = overlayTf.GetComponent<Image>();

        if (_cooldownOverlay == null)
        {
            var go = new GameObject("Cooldown", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);

            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            _cooldownOverlay = go.GetComponent<Image>();
            _cooldownOverlay.raycastTarget = false;
            _cooldownOverlay.color = new Color(0f, 0f, 0f, 0.55f);

            _cooldownOverlay.sprite = GetOrCreateCircleSprite(); //  내장 리소스 의존 X
            _cooldownOverlay.type = Image.Type.Filled;
            _cooldownOverlay.fillMethod = Image.FillMethod.Radial360;
            _cooldownOverlay.fillOrigin = (int)Image.Origin360.Top;
            _cooldownOverlay.fillClockwise = false;
            _cooldownOverlay.fillAmount = 0f;
            _cooldownOverlay.enabled = false;
        }
        ShrinkOverlay(0.92f);  // 92% 크기 = 둘레 8% 여백

        void ShrinkOverlay(float scale)
        {
            var btnRt = (RectTransform)transform;
            var rt = _cooldownOverlay.rectTransform;

            // 가운데 기준 고정 크기로 바꿔서 지름*scale 적용
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);

            // 현재 버튼의 짧은 변을 기준으로 원 크기 결정
            float d = Mathf.Min(btnRt.rect.width, btnRt.rect.height);
            rt.sizeDelta = new Vector2(d * scale, d * scale);
        }
        // 텍스트
        var textTf = transform.Find("CooldownText");
        if (textTf != null)
            _cdText = textTf.GetComponent<TextMeshProUGUI>();

        if (_cdText == null)
        {
            var goText = new GameObject("CooldownText", typeof(RectTransform), typeof(TextMeshProUGUI));
            goText.transform.SetParent(transform, false);

            var rt = (RectTransform)goText.transform;
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

            _cdText = goText.GetComponent<TextMeshProUGUI>();
            _cdText.raycastTarget = false;
            _cdText.alignment = TextAlignmentOptions.Center;
            _cdText.fontSize = 60; // 필요시 조절
            _cdText.color = new Color(1f, 1f, 1f, 0.9f);
            _cdText.outlineWidth = 0.25f;
            _cdText.outlineColor = new Color(0f, 0f, 0f, 0.9f);
            _cdText.enabled = false;
        }
    }

    // **프로시저 원형 스프라이트(1회 생성 캐시)**
    private static Sprite GetOrCreateCircleSprite()
    {
        if (_circleSpriteCache != null) return _circleSpriteCache;

        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.ARGB32, false);
        tex.wrapMode = TextureWrapMode.Clamp;

        var pixels = new Color32[size * size];
        float r = (size - 1) * 0.5f;
        float r2 = r * r;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - r;
                float dy = y - r;
                bool inside = dx * dx + dy * dy <= r2;
                pixels[y * size + x] = inside
                    ? new Color32(255, 255, 255, 255)
                    : new Color32(255, 255, 255, 0);
            }
        }
        tex.SetPixels32(pixels);
        tex.Apply();

        _circleSpriteCache = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        _circleSpriteCache.name = "GeneratedCircle64";
        return _circleSpriteCache;
    }
}
