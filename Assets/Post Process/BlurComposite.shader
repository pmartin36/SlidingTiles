Shader "Hidden/BlurComposite"
{
	HLSLINCLUDE
		#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		sampler2D _GlowTex;
		float _Intensity;
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

			float4 Frag(VaryingsDefault i) : SV_Target
			{
				float4 base = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
				float4 second = tex2D(_GlowTex, i.texcoord);
				return base + second * _Intensity;
			}

			ENDHLSL
		}
	}
}
