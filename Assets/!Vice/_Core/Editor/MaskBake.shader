Shader "Hidden/Vice/MaskBake"
{
    Properties
    {
        _MainTex ("Base", 2D) = "white" {}
        _Mask ("Mask", 2D) = "black" {}
    }

    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment Frag
            #include "UnityCG.cginc"

            // both textures arrive as raw bytes; the bake does every colour-space conversion itself
            sampler2D _MainTex;
            sampler2D _Mask;
            float _BaseIsSRGB;
            float _MaskIsSRGB;

            // linear-space colours, already converted on the C# side
            float4 _BaseColorValue;
            float4 _MaskTint;
            float4 _DirtColor;
            float4 _LeakColor;
            float4 _EdgeDamageColor;
            float _MaskColorEnabled;
            float _MaskDirtEnabled;

            float3 SRGBToLinear(float3 encoded)
            {
                float3 low = encoded / 12.92;
                float3 high = pow((encoded + 0.055) / 1.055, 2.4);
                return lerp(high, low, step(encoded, 0.04045));
            }

            float3 LinearToSRGB(float3 linearColor)
            {
                float3 low = linearColor * 12.92;
                float3 high = 1.055 * pow(linearColor, 1.0 / 2.4) - 0.055;
                return lerp(high, low, step(linearColor, 0.0031308));
            }

            float4 Frag(v2f_img IN) : SV_Target
            {
                float3 base = tex2D(_MainTex, IN.uv).rgb;
                base = _BaseIsSRGB > 0.5 ? SRGBToLinear(base) : base;

                float4 mask = tex2D(_Mask, IN.uv);
                // the GPU's sRGB decode touches rgb only, so alpha stays raw exactly as it does at runtime
                mask.rgb = _MaskIsSRGB > 0.5 ? SRGBToLinear(mask.rgb) : mask.rgb;

                // same lerp chain and order as S_GeneralORM: later layers paint over earlier ones
                float3 color = base * _BaseColorValue.rgb;
                color = lerp(color, _MaskTint.rgb, mask.r * _MaskColorEnabled);
                color = lerp(color, _DirtColor.rgb, mask.g * _MaskDirtEnabled);
                color = lerp(color, _LeakColor.rgb, mask.b * _MaskDirtEnabled);
                color = lerp(color, _EdgeDamageColor.rgb, mask.a * _MaskDirtEnabled);

                // encode before the 8-bit render target quantizes, so dark tones keep their precision
                return float4(LinearToSRGB(saturate(color)), 1.0);
            }
            ENDCG
        }
    }
}
