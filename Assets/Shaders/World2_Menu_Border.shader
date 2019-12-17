Shader "SlidingTiles/World2MenuBorder"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Map("Map Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_LightColor("Light Color", Color) = (0.5,0,0,1)
		_Tolerance("Tolerance", Range(0, 0.3)) = 0.18
		_MaxLightValue("Max Light",  Range(1,2)) = 2
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
			ZTest Always
			ZWrite Off

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
				fixed4 color : COLOR0;
            };

            sampler2D _MainTex;
			sampler2D _Map;
            float4 _MainTex_ST;
			float4 _Color;
			float4 _LightColor;	
			float _Tolerance;
			float _MaxLightValue;

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

				fixed4 l = tex2D(_Map, i.uv);
				float time = fmod(_Time.y * 0.75, _MaxLightValue);
				float fade = l.b / 10;
				float lightVal = frac(l.r * 2);

				float s = sign(lightVal - time);		
				lightVal += sign(lightVal - time) * fade;

				float diff = fmod(abs(time - lightVal), 2);	
				float4 color = lerp(_LightColor, float4(1, 1, 1, 1), l.g);
				color.rgb *= l.a;
				color.a = l.a;
				col += (1-step(_Tolerance, diff)) *color;
				
				return col;
            }
            ENDCG
        }
    }
}
