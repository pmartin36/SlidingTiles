Shader "SlidingTiles/Points"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Noise("Noise", 2D) = "white" {}
	}
		SubShader
	{
		Tags {
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
		}

		Pass
		{
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			static const fixed PI = 3.1415926;

			float vectorToAngle(float2 xy) {
				return degrees(atan2(xy.y, xy.x));
			}

			float inverseLerp(float a, float b, float v) {
				return (v - a) / (b - a);
			}

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color: COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _Color;
			sampler2D _Noise;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color * _Color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 remappedUv = i.uv * 2 - 1;

				float radius = length(remappedUv);
				float theta = atan2(-remappedUv.y, -remappedUv.x);
				float theta01 = (theta + PI) / (PI * 2);

				float puvx = radius + (_Time.y + theta);
				float puvy = theta01 * (8);
				float2 puv = float2(puvx, puvy);

				fixed4 noise = fixed4(tex2D(_Noise, puv).rgb, 1);

				float stepVal = radius - 0.8;
				return (1 - saturate(stepVal / fwidth(stepVal))) * noise;
			}
			ENDCG
		}
	}
}
