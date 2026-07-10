Shader "Custom/OutlineShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Front

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normal : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normal : TEXCOORD0;
            };


            TEXTURE2D(_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
            CBUFFER_END


            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normal = TransformObjectToWorldNormal(IN.normal);
                float3 clipNormal = TransformWorldToHClipDir(OUT.normal);
                OUT.positionHCS.xy += clipNormal.xy * 0.04f;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float4 color = _BaseColor;
                return color;
            }
            ENDHLSL
        }
    }
}
