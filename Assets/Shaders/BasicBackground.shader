Shader "SlidingTiles/BasicBackground"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)	
		_SecondaryColor("Selection Color", Color) = (0.5,0,0,1)
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
				fixed4 primaryColor : COLOR0;
				fixed4 secondaryColor : COLOR1;
            };

            sampler2D _MainTex;
			sampler2D _Noise;
            float4 _MainTex_ST;
			float4 _Color;
			float4 _SecondaryColor;		

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.primaryColor = v.color * _Color;
				o.secondaryColor = v.color * _SecondaryColor;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {		
                // sample the texture
				//fixed4 noise = tex2D(_Noise, i.uv - float2(0, _Time.x )).r * 0.2 - 0.05;	
				
				fixed4 noise = tex2D(_Noise, i.uv - float2(0, _Time.x )).r * 0.3 - 0.15;	
				float factor = min(smoothstep(0, 0.5, i.uv.y), smoothstep(1, 0.5, i.uv.y));
				noise *= factor;

				/*fixed4 noise1 = tex2D(_Noise, i.uv - float2(_Time.x, 0 )).r;
				fixed4 noise2 = tex2D(_Noise, i.uv - float2(-_Time.x, 0 )).r;
				fixed4 noise = min(noise1, noise2) * 0.3 - 0.1;

				float factor = min(smoothstep(0, 0.5, i.uv.y), smoothstep(1, 0.5, i.uv.y)); 
				noise *= factor * 1.5;*/


				return lerp(i.primaryColor, i.secondaryColor, i.uv.y + noise);
            }
            ENDCG
        }
    }
}
