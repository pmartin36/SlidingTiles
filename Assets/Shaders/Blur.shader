Shader "Hidden/Blur"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always


		// Horizontal
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#define KERNEL_SIZE 7

			static const float weights[KERNEL_SIZE] = { 0.00598, 0.060626, 0.241843, 0.383103, 0.241843, 0.060626, 0.00598 };

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float2 _BlurSize;

			fixed4 frag(v2f i) : SV_Target
			{
				float4 col = float4(0, 0, 0, 0);
				const int iter = (KERNEL_SIZE - 1) / 2;

				for (int j = -iter; j <= iter; j++) {
					float weight = weights[j + iter];
					float4 s = tex2D(_MainTex, i.uv + float2(j * _BlurSize.x, 0));
					col += s * weight;
				}

				return col;
			}
			ENDCG
		}

		// Vertical
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			#define KERNEL_SIZE 7

			static const float weights[KERNEL_SIZE] = { 0.00598, 0.060626, 0.241843, 0.383103, 0.241843, 0.060626, 0.00598 };

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			float2 _BlurSize;

			fixed4 frag(v2f i) : SV_Target
			{
				float4 col = float4(0, 0, 0, 0);
				const int iter = (KERNEL_SIZE - 1) / 2;

				for (int j = -iter; j <= iter; j++) {
					float weight = weights[j + iter];
					float4 s = tex2D(_MainTex, i.uv + float2(0, j * _BlurSize.y));
					col += s * weight;
				}

				return col;
			}
			ENDCG
		}
	}
}