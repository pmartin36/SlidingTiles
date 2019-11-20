Shader "SlidingTiles/ScreenWipe"
{
	HLSLINCLUDE
		#include "Packages/com.unity.postprocessing/PostProcessing/Shaders/StdLib.hlsl"

		TEXTURE2D_SAMPLER2D(_MainTex, sampler_MainTex);
		float _Percent;
		sampler2D _SecondLevelTexture;

		float4 Frag(VaryingsDefault i) : SV_Target
		{
			float p = _Percent * 1.4 - 0.2;
			float dividerVal = i.texcoord.x;
			float add = pow(1 - saturate(abs(dividerVal - p)), 30);

			float4 base = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.texcoord);
			float4 second = tex2D(_SecondLevelTexture, i.texcoord);
			float stepVal = step(p, dividerVal);

			float4 col = lerp(base, second, stepVal) + float4(1, 1, 1, 0) * add;
			col.a = 1;
			return col;
		}
	ENDHLSL

	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			HLSLPROGRAM

				#pragma vertex VertDefault
				#pragma fragment Frag

			ENDHLSL
		}
	}
}
