void HitFlash_float(UnityTexture2D MainTex, float2 UV, float4 Tint, float _HitFlash, out float4 RGBA)
{
    float4 baseCol = MainTex.Sample(MainTex.samplerstate, UV);
    float f = saturate(_HitFlash);
    float3 rgb = lerp(baseCol.rgb, Tint.rgb, f);
    RGBA = float4(rgb, baseCol.a);
}
