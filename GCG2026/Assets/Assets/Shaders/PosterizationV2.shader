Shader "Custom/PosterizationV2" {
	Properties {
		// 色の階調
		_step ("Step", float) = 20
		// 色のオフセット
		_offset ("Offset", float) = 0.0
		// 明度の最小値
		_minValue ("Min Value", float) = 0.0
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
				float _offset;
				float _minValue;
			CBUFFER_END

			// ポスタライズ処理
			float Posterize(float input, float step)
			{
				return floor(input * step + 0.5) / step;
			}

			// RGVをHSVに変換する
			half4 RGBToHSV(half4 rgb)
			{
				float maxValue = max(max(rgb.r, rgb.g), rgb.b);
				float minValue = min(min(rgb.r, rgb.g), rgb.b);

				// 色相
				float hue = 0.0;
				if (maxValue != minValue && maxValue > 0)
				{
					if (rgb.r == maxValue)
					{
						hue = ((rgb.g - rgb.b) / (maxValue - minValue)) * 60;
					}
					else if (rgb.g == maxValue)
					{
						hue = ((rgb.b - rgb.r) / (maxValue - minValue)) * 60 + 120;
					}
					else if (rgb.b == maxValue)
					{
						hue = ((rgb.r - rgb.g) / (maxValue - minValue)) * 60 + 240;
					}
					else if (rgb.r == rgb.g && rgb.g == rgb.b)
					{
						hue = 0.0;
					}
				}
				

				if (hue < 0)
					hue += 360;

				// 彩度
				float saturation = 0.0f;
				if (maxValue > 0)
					saturation = (maxValue - minValue) / maxValue;
					
				// 明度
				float value = maxValue;

				half4 hsv;
				hsv.x = hue / 360.0;
				hsv.y = saturation;
				hsv.z = value;
				hsv.w = rgb.w;

				return hsv;
			}

			// HSVをRBGに変換する
			half4 HSVToRGB(half4 hsv)
			{
				half4 rgb;

				float hue = hsv.x * 360;
				float saturation = hsv.y;
				float value = hsv.z;

				float maxValue = value;
				float minValue = maxValue - ((saturation / 1.0f) * maxValue);

				if (hue >= 0 && hue < 60)
				{
					rgb.r = maxValue;
					rgb.g = (hue / 60) * (maxValue - minValue) + minValue;
					rgb.b = minValue;
				}
				else if (hue >= 60 && hue < 120)
				{
					rgb.r = ((120 - hue) / 60) * (maxValue - minValue) + minValue;
					rgb.g = maxValue;
					rgb.b = minValue;
				}
				else if (hue >= 120 && hue < 180)
				{
					rgb.r = minValue;
					rgb.g = maxValue;
					rgb.b = ((hue - 120) / 60) * (maxValue - minValue) + minValue;
				}
				else if (hue >= 180 && hue < 240)
				{
					rgb.r = minValue;
					rgb.g = ((240 - hue) / 60) * (maxValue - minValue) + minValue;
					rgb.b = maxValue;
				}
				else if (hue >= 240 && hue < 300)
				{
					rgb.r = ((hue - 240) / 60) * (maxValue - minValue) + minValue;
					rgb.g = minValue;
					rgb.b = maxValue;
				}
				else if (hue >= 300 && hue < 360)
				{
					rgb.r = maxValue;
					rgb.g = minValue;
					rgb.b = ((360 - hue) / 60) * (maxValue - minValue) + minValue;
				}

				rgb.w = hsv.w;

				return rgb;
			}

			half4 Frag(Varyings input) : SV_Target
			{
				// VR画面ズレ防止
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				// 現在、画面のどこを処理しているのかを、0.0～1.0の間で取得
				float2 uv = input.texcoord;

				// 元の画面の色をサンプリング
				half4 originalRGB = SAMPLE_TEXTURE2D_X(
					_BlitTexture,			// 画面バッファ
					sampler_LinearClamp,	// どういう設定でサンプリングするか?今回はクランプ
					uv);					// 画面のどこを処理しているのかをuv座標で渡す。

				// HSVに変換
				half4 originalHSV = RGBToHSV(originalRGB);

				// ポスタライズ（減色処理）適用
				half4 posterizedHSV;
				posterizedHSV.x = originalHSV.x;
				posterizedHSV.y = originalHSV.y;
				posterizedHSV.z = Posterize(originalHSV.z, _step) + _offset;

				if (posterizedHSV.z <= _minValue) posterizedHSV.z = _minValue;

				posterizedHSV.w = originalHSV.w;

				// RGBに変換
				half4 posterizedRGB = HSVToRGB(posterizedHSV);

				return posterizedRGB;
			}
			ENDHLSL
		}
	}
}