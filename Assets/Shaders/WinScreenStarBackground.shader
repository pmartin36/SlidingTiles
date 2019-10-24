Shader "SlidingTiles/WinScreenStarBackground"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_ShineColor("Shine Color", Color) = (1,1,1,1)
		_Noise("Noise", 2D) = "white" {}
		_PctAnimated("Percent", Range(0,1)) = 0
		_RadiusModifier("Radius Modifier", Range(-0.5, 0.5)) = 0
    }
    SubShader
    {
        Tags { 
			"RenderType"="Transparent" 
			"Queue"="Transparent"
		}

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
			Cull Off

            CGPROGRAM	
            #pragma vertex vert
            #pragma fragment frag
			
            #include "UnityCG.cginc"

			static const fixed PI = 3.1415926;

			float inverseLerp(float a, float b, float v) {
				return (v - a) / (b - a);
			}

			float2 rotate(float2 o, float r) {
				float c = cos(r);
				float s = sin(r);
				return float2(o.x * c - o.y * s, o.x * s + o.y * c);
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
			sampler2D _Noise;
			float4 _Color;	
			float4 _ShineColor;		
			float4 _HiddenColor;
			float _PctAnimated;
			float _RadiusModifier;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {	
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;	

				float center = _PctAnimated * 4 - 2;
				//center =  fmod(_Time.y, 4) - 2; // -4 to 4
				float l = center + i.uv.x - i.uv.y;

				float2 remappedUv = i.uv * 2 - 1;

				float radius = length(remappedUv);
				float theta = atan2(-remappedUv.y, -remappedUv.x);
				float theta01 = (theta + PI) / (PI * 2);

				fixed noise = tex2D(_Noise, i.uv).r;

				float tVal = theta01 * 50 + _Time.y + noise * _SinTime.y;
				float m = min(frac(tVal), frac(1 - tVal));
				noise = smoothstep(0, 0.5, m * (1.2 - radius + _RadiusModifier));
				noise += (0.6 - radius + _RadiusModifier) * 1.5;
							
				float tVal2 = (theta01 + 0.1) * 35 + _Time.y;
				float m2 = min(frac(tVal2), frac(1 - tVal2));
				float alpha = inverseLerp(0.4, 0.7, noise * m2);			
				float4 cAlpha = float4(1, 1, 1, alpha * 1.5 + col.a);

				float4 color = float4(i.color.rgb, 1);
				float lVal = inverseLerp(0.9, 1.1, noise);
				col = 
					(1 - step(l, 0)) // from percent animation
					* (	lerp(color, float4(1, 1, 1, 1), lVal) * cAlpha 
						+ smoothstep(0.5, 0.2, radius) * float4(1,1,1,1) );
				return saturate(col);
            }
            ENDCG
        }
    }
}
