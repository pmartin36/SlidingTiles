Shader "SlidingTiles/WinScreenStar"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_ShineColor("Shine Color", Color) = (1,1,1,1)
		_HiddenColor("Hidden Color", Color) = (1,1,1,1)
		_Noise("Noise", 2D) = "white" {}
		_PctAnimated("Percent", Range(0,1)) = 0
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
				float4 cAlpha = float4(1, 1, 1, col.a);

				float center = _PctAnimated * 4 - 2;
				// center =  fmod(_Time.y, 4) - 2; // -4 to 4
				float l = center + i.uv.x - i.uv.y;
				float lsqr = pow(l, 2);

				float lerpVal = 1 - saturate(abs(l));
				float lerpSqr = pow(lerpVal, 2);

				// l < 0, l = 20
				// l > 0, l = -2
				float m = 20 - step(0, l) * 22;
				fixed4 noise = tex2D(_Noise, i.uv * 3 + _Time.x * m * float2(1, -1) + lsqr * _SinTime.y * 0.2 * i.uv.x);
				noise = (noise * 2) - 1;
			
				float4 add = _ShineColor * (lerpSqr + noise * pow(lerpSqr, 2) * lerp(0.6, 0.0, lerpSqr)) * cAlpha;
				float4 hiddenColor = _HiddenColor * cAlpha;
				col = lerp(hiddenColor, col, saturate(l * 5 + 0.2));
				//return col;

				return col + add;
            }
            ENDCG
        }
    }
}
