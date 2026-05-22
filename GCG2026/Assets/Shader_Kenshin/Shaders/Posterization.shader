Shader "Custom/Posterization" {
	// Properties {}
	SubShader {
		Tags {
			"RenderPipeline" = "UniversalPipeline"
			"RenderType" = "Opaque"
		}

		Pass {
			CGPROGRAM
			// #pragma vertex Vertex
			#pragma fragment Fragment

			// #include "UnityCG.cginc"

			// struct Attributes {
			// 	float3 positionLS : POSITION;
			// };

			// struct Varyings {
			// 	float4 positionCS : SV_POSITION;
			// };

			// void Posterize_float4(float4 In, float4 Steps, out float4 Out) {
			// 	Out = floor(In / (1 / Steps)) * (1 / Steps);
			// }

			// Varyings Vertex(Attributes a) {
			// 	Varyings output;
			// 	return output;
			// }

			// float _DivideNum;

			fixed4 Fragment() : SV_Target {
				return half4(1, 0, 0, 1);
			}
			ENDCG
		}
	}
}