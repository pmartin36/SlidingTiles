Shader "SlidingTiles/Preview2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_PreviewWorldPosition("Preview World Space Position", Vector) = (0,0,0,0)
    }
    SubShader
    {
        Tags { 
			"RenderType"="Transparent" 
			"Queue"="Transparent+1"
		}

		GrabPass
		{
			"_BackgroundTexture"
		}

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

            CGPROGRAM	
            #pragma vertex vert
            #pragma fragment frag
			
            #include "UnityCG.cginc"

			float2 rotate(float2 o, float r) {
				float c = cos(r);
				float s = sin(r);
				return float2(o.x * c - o.y * s, o.x * s + o.y * c);
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
				float4 grabPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			sampler2D _BackgroundTexture;
			float4 _Color;
			float4 _PreviewWorldPosition;
			static const fixed PI = 3.1415926;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				float4 scale = float4( 
					2 * length(UNITY_MATRIX_M._m00_m01_m02),
					2 * length(UNITY_MATRIX_M._m10_m11_m12),
					2 * length(UNITY_MATRIX_M._m20_m21_m22),
					1 / v.vertex.w
				);

				float4 vScaled = v.vertex * scale;
				float4 samplePosition = _PreviewWorldPosition + vScaled;

				o.grabPos = mul(UNITY_MATRIX_VP, samplePosition);
				o.grabPos = ComputeGrabScreenPos(o.grabPos);

				o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {		
				float2 remappedUv = i.uv * 2 - 1;
				float maxxy = max(abs(remappedUv.x), abs(remappedUv.y));

				float radius = length(remappedUv);
				float theta = atan2(-remappedUv.y, -remappedUv.x);
				float theta01 = (theta + PI) / (PI * 2);

				float puvx = radius - (_Time.x * 4);
				float puvy = theta01 * 10;
				float2 puv = float2(puvx * 2, puvy);

				fixed4 tex = fixed4(tex2D(_MainTex, puv / 8).rgb, 1);
				float alphaValMultiplier = smoothstep(1.2, 0.6, length(maxxy) * radius);
				tex = (1, 1, 1, (inverseLerp(0.2, 0.6, tex.r)) * alphaValMultiplier);

				fixed4 gp = tex2D(_BackgroundTexture, i.grabPos.xy);
				float lerpVal = smoothstep(0.7, 0.9, maxxy);
				fixed4 col = lerp(gp, tex, lerpVal);
				col *= i.color;
				return col;
            }
            ENDCG
        }
    }
}
