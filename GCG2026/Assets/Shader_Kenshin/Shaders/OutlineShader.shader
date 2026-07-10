Shader "Custom/OutlineShader"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (0, 0, 0, 0)
        _LineWidth("Line Width", float) = 0.03
    }

    SubShader
    {
        // Tags { "RenderType" = "Opaque" }
        ZWrite Off

        Pass
        {
            // Name "Pass1"
            Stencil{
                Ref 1
                Comp Always
                Pass replace
            }
            Cull Off
            ColorMask 0
            // Cull Back
            // ZTest Always



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
                float _LineWidth;
            CBUFFER_END


            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // IN.positionOS.xyz += IN.positionOS.xyz * 0.1f;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normal = TransformObjectToWorldNormal(IN.normal);
                float3 clipNormal = TransformWorldToHClipDir(OUT.normal);
                // OUT.positionHCS.xy += clipNormal.xy * 0.1f;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // float4 color = _BaseColor;
                return float4(0.0f, 0.0f, 0.0f, 1.0f);
            }
            ENDHLSL
        }

        Pass
        {
            Stencil{
                Ref 1
                Comp NotEqual
                Pass Replace
            }
            Cull Off
            // Cull Back
            // ZTest Always



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
                float _LineWidth;
            CBUFFER_END


            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                IN.positionOS.xyz += IN.positionOS.xyz * _LineWidth;
    
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normal = TransformObjectToWorldNormal(IN.normal);
                // float3 clipNormal = TransformWorldToHClipDir(OUT.normal);
                // float3 
                // OUT.positionHCS.xy += clipNormal.xy * _LineWidth;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // float4 color = _BaseColor;
                return _BaseColor;
            }
            ENDHLSL
        }
    }
}
