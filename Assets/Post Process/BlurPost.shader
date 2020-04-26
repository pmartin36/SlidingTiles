Shader "Hidden/BlurPost"
{
	HLSLINCLUDE
		#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"
		#define KERNEL_SIZE 7

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		float2 _BlurSize;
	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		// Horizontal
		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment Frag

			static const float weights[KERNEL_SIZE] = { 0.00598, 0.060626, 0.241843, 0.383103, 0.241843, 0.060626, 0.00598 };

			float4 Frag(VaryingsDefault i) : SV_Target
			{
				float4 col = float4(0, 0, 0, 0);
				const int iter = (KERNEL_SIZE - 1) / 2;

				for (int j = -iter; j <= iter; j++) {
					float weight = weights[j + iter];
					float4 s = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(j * _BlurSize.x, 0));
					col += s * weight;
				}

				return col;
			}

			ENDHLSL
		}

		// Vertical
		Pass
		{
			HLSLPROGRAM

			#pragma vertex VertDefault
			#pragma fragment Frag

			static const float weights[KERNEL_SIZE] = { 0.00598, 0.060626, 0.241843, 0.383103, 0.241843, 0.060626, 0.00598 };

			float4 Frag(VaryingsDefault i) : SV_Target
			{
				float4 col = float4(0, 0, 0, 0);
				const int iter = (KERNEL_SIZE - 1) / 2;

				for (int j = -iter; j <= iter; j++) {
					float weight = weights[j + iter];
					float4 s = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord + float2(0, j * _BlurSize.y));
					col += s * weight;
				}

				return col;
			}

			ENDHLSL
		}
	}
}
