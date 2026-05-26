Shader "Custom/URPUnlit" {
// 	Properties {}
	SubShader {
		Tags {
			"RenderPipeline" = "UniversalPipeline"
			"RenderType" = "Opaque"
		}

		Pass {
			Name "UnlitPass"

			HLSLPROGRAM
			#pragma vertex Vertex
			#pragma fragment Fragment

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes {
				float3 positionLS : POSITION;
			};

			struct Varyings {
				float4 positionCS : SV_POSITION;
			};

			Varyings Vertex(Attributes a) {
				Varyings output;
				// local space -> clip space
				output.positionCS = TransformObjectToHClip(a.positionLS);
				return output;
			}

			half4 Fragment() : SV_Target {
				return half4(1, 1, 0, 1);
			}
			ENDHLSL
		}
	}
}