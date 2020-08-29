Shader "SlidingTiles/SpawnIndicator2"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_CenterColor("Center Color", Color) = (1,1,1,1)
		_HighlightColor("Highlight Color", Color) = (1,1,1,1)
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
				fixed4 centerColor : COLOR;
				fixed4 highlightColor : COLOR1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _CenterColor;
			float4 _HighlightColor;
			sampler2D _Noise;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.centerColor = v.color * _CenterColor;
				o.highlightColor = v.color * _HighlightColor;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv);
				float2 remappedUv = i.uv * 2 - 1;

				float radius = length(remappedUv);
				float theta = atan2(-remappedUv.y, -remappedUv.x);
				float theta01 = (theta + PI) / (PI * 2);

				float targetTheta = frac(_Time.y * 0.75);
				float tThreshold = 0.6;
				float maxTheta = targetTheta + tThreshold;

				//float minTheta = targetTheta - tThreshold;
				//theta01 += step(0.95, maxTheta - theta01); // wrapping around 0
				//float ss = saturate(
				//	smoothstep(maxTheta, targetTheta, theta01)
				//	- (1 - step(targetTheta, theta01 ))
				//);

				 float ss = 0;
				 float minTheta = targetTheta - 0.8;

				theta01 -= step(0.95, theta01 - minTheta); // wrapping around 0
				ss += saturate(
					smoothstep(minTheta, targetTheta, theta01)
					- step(targetTheta, theta01)
				);

				float radiusAlpha = smoothstep(0.95, 0.82, radius);		
				col = lerp(
					i.centerColor,
					i.highlightColor,
					smoothstep(1.1, 0.7, ss)
				);
				col.a *= radiusAlpha * inverseLerp(0, 0.5, ss);

				return col;
			}
			ENDCG
		}
	}
}
