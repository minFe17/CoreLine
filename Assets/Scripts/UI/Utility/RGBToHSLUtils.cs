using UnityEditor;
using UnityEngine;

public static class RGBToHSLUtils
{
    private static Color _color;

    public static Color Color
    {
        get { return _color; }
        set { _color = value; }
    }
    // RGB -> HSL
    public static void RGBToHSL(Color rgb, out float h, out float s, out float l)
    {
        float r = rgb.r;
        float g = rgb.g;
        float b = rgb.b;

        float max = Mathf.Max(r, Mathf.Max(g, b));
        float min = Mathf.Min(r, Mathf.Min(g, b));
        h = s = l = (max + min) / 2f;

        if (Mathf.Approximately(max, min))
        {
            h = s = 0f; // 무채색
        }
        else
        {
            float d = max - min;
            s = l > 0.5f ? d / (2f - max - min) : d / (max + min);

            if (max == r)
                h = (g - b) / d + (g < b ? 6f : 0f);
            else if (max == g)
                h = (b - r) / d + 2f;
            else
                h = (r - g) / d + 4f;

            h /= 6f;
        }
    }
    // HSL -> RGB
    public static Color HSLToRGB(float h, float s, float l, float alpha = 1f)
    {
        float r, g, b;

        if (Mathf.Approximately(s, 0f))
        {
            r = g = b = l; // 무채색
        }
        else
        {
            float q = l < 0.5f ? l * (1f + s) : l + s - l * s;
            float p = 2f * l - q;

            r = HueToRGB(p, q, h + 1f / 3f);
            g = HueToRGB(p, q, h);
            b = HueToRGB(p, q, h - 1f / 3f);
        }

        return new Color(r, g, b, alpha);
    }
    private static float HueToRGB(float p, float q, float t)
    {
        if (t < 0f) t += 1f;
        if (t > 1f) t -= 1f;
        if (t < 1f / 6f) return p + (q - p) * 6f * t;
        if (t < 1f / 2f) return q;
        if (t < 2f / 3f) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }
    // 밝기 조절: percent는 -1.0f ~ +1.0f
    public static Color AdjustLightness(Color original, float percent)
    {
        RGBToHSL(original, out float h, out float s, out float l);
        l = Mathf.Clamp01(l + percent);
        return HSLToRGB(h, s, l, original.a);
    }

    public static Color Lighten(Color original, float percent)
    {
        return AdjustLightness(original, Mathf.Abs(percent));
    }

    public static Color Darken(Color original, float percent)
    {
        return AdjustLightness(original, -Mathf.Abs(percent));
    }
}