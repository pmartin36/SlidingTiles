Shader "SlidingTiles/SelectedTile"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)	
		_SelectionColor("Selection Color", Color) = (0.5,0,0,1)
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
			sampler2D _Noise;
            float4 _MainTex_ST;
			float4 _Color;
			float4 _SelectionColor;		

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
                fixed4 col = tex2D(_MainTex, i.uv);
				float len = length((i.uv) * 2 - 1);
				fixed4 noise = (tex2D(_Noise, i.uv) * 2 - 1) ;
				col.rgb = lerp(
					col.rgb, 
					max(_SelectionColor.rgb, col.rgb),
					smoothstep(
						0.9, 
						1.4, 
						len + sin(_Time.y * 2) * 0.2 + noise * 0.2
					)
				);
				return col;
            }
            ENDCG
        }
    }
}
