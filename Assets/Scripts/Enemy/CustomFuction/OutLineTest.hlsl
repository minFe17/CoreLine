void OutLineTest_float(float2 UV,UnityTexture2D MainTex,out float4 result)
{
    float2 imageSize = float2(1920,1080);
    float4 outlineColor = float4(1, 0, 0, 1); 
    float outlineSize = 10.1;
    float alpha = MainTex.Sample(MainTex.samplerstate, UV).a;
    float isOutline = 0.0;

    for (int x = -1; x <= 1; x++)
    {
        for (int y = -1; y <= 1; y++)
        {
            if(x == 0 && y == 0) continue;
            float2 offset = float2(x, y) * (outlineSize / imageSize);
            float a = MainTex.Sample(MainTex.samplerstate, clamp(UV + offset, 0.0, 1.0)).a;
            if (alpha > 0.1 && a < 0.1)
            {
                isOutline = 1.0;
            }
        }
    }

    float4 baseColor = MainTex.Sample(MainTex.samplerstate, UV);
    result = lerp(baseColor, outlineColor, isOutline);
}