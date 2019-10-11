Shader "SlidingTiles/Points2"
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
				float puvy = theta01 * (8) * radius;

				float theta1 = (theta01 + 0.66 * _Time.x * 2) * (8);
				float theta2 = (theta01 - 0.40 * _Time.x * 2) * (8);
				float theta3 = (theta01 + 0.28 * _Time.x * 2) * (8);

				float m1 = min(frac(theta1), frac(1 - theta1));
				float m2 = min(frac(theta2), frac(1 - theta2));
				float m3 = min(frac(theta3), frac(1 - theta3));

				//float x = ((_SinTime.y * 3 + 1) / 2) * 0.2 + 0.5;
				float x = 0.4 * radius;
				float b1 = 0.48 + (sin(_Time.x * 6) + 1)  * 0.04;
				float b2 = 0.48 + (sin(_Time.x * 10) + 1) * 0.04;
				float b3 = 0.48 + (sin(_Time.x * 16) + 1) * 0.04;

				float stepVal1 = smoothstep(0, 1, m1*x + b1) - radius;// (m*0.5 + 0.7);
				float stepVal2 = smoothstep(0, 1, m2*x + b2) - radius;// (m*0.5 + 0.7);
				float stepVal3 = smoothstep(0, 1, m3*x + b3) - radius;// (m*0.5 + 0.7);

				float s1 = saturate(stepVal1 / fwidth(stepVal1));
				float s2 = saturate(stepVal2 / fwidth(stepVal2));
				float s3 = saturate(stepVal3 / fwidth(stepVal3));
				
				float3 stepVal = (s1 * 0.3 * float3(2,1,1) + s2 * 0.4 * float3(1,2,1) + s3 * 0.5 * float3(1,1,2)) * i.color.rgb;
				return float4(stepVal, stepVal.r + stepVal.g + stepVal.b);


				//return saturate(stepVal / fwidth(stepVal));
				return
					(float4	(i.color.r * s1, 0, 0, s1 * 0.66)
					+ float4(0, i.color.g * s2, 0, s2 * 0.40)
					+ float4(0, 0, i.color.b * s3, s3 * 0.28));
			}
			ENDCG
		}
	}
}
