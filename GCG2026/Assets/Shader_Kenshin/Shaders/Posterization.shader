Shader "Custom/Posterization" {
	Properties {
		// 色の階調
		_step ("Step", float) = 20
	}
	SubShader {
		Tags {
			"RenderPipeline" = "UniversalPipeline"
			"RenderType" = "Opaque"
		}
		LOD 100
		// フルスクリーン描画用設定
		ZTest Always
		ZWrite Off
		Cull Off

		Pass {
			Name "PosterizeFullscreen"

			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment Frag

			// インクルード
			// #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			// _BlitTexture（画面バッファ）定義
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
			// Varyings定義
			#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

			// SRPBatcherに対応させるため、Propertiesで宣言した変数をHLSL内でも宣言しておく。
			CBUFFER_START(UntiyPerMaterial)
				float _step;
			CBUFFER_END

			// ポスタライズ処理
			float Posterize(float input, float step)
			{
				return floor(input * step + 0.5) / step;
			}

			half4 Frag(Varyings input) : SV_Target
			{
				// VR画面ズレ防止
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				// 現在、画面のどこを処理しているのかを、0.0～1.0の間で取得
				float2 uv = input.texcoord;

				// 元の画面の色をサンプリング
				half4 originalColor = SAMPLE_TEXTURE2D_X(
					_BlitTexture,			// 画面バッファ
					sampler_LinearClamp,	// どういう設定でサンプリングするか?今回はクランプ
					uv);					// 画面のどこを処理しているのかをuv座標で渡す。

				// ポスタライズ（減色処理）適用
				half4 posterized;
				posterized.r = Posterize(originalColor.r, _step);
				posterized.g = Posterize(originalColor.g, _step);
				posterized.b = Posterize(originalColor.b, _step);

				return posterized;
			}
			ENDHLSL
		}
	}
}