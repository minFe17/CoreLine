void OutLine_float(UnityTexture2D MainTex, float2 UV, float4 Color, float Weight, float2 Size, out float4 RGBA)
{
    float4 baseColor = MainTex.Sample(MainTex.samplerstate, UV);    
    
    float count = 0.0f;

    for(int y = -1 ; y <= 1; y++)
    {
        for(int x = -1; x <= 1; x++)
        {
            float2 offset = (float2(x, y) / Size) * Weight;            
            count += MainTex.Sample(MainTex.samplerstate, UV + offset).a;
        }
    }

    float outlineFactor = (count > 0.0f && count < 9.0f) ? 1.0f : 0.0f;

    RGBA = float4(Color.rgb, Color.a * outlineFactor);

    //[branch]
    //if(count > 0.0f && count < 9)
    //{
    //    RGBA = Color;
    //}
    //else
    //{
    //    RGBA = float4(0, 0, 0, 0);
    //}
}