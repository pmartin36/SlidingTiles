Shader "SlidingTiles/Platform"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		[HideInInspector] _Rotation("Rotation", float) = 0
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
				float4 world: TEXCOORD1;
				float2 bUv: TEXCOORD2;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _Target;
			float4 _Color;
			float _Rotation;

			float2 rotate(float2 o, float r) {
				float c = cos(r);
				float s = sin(r);
				return float2(o.x * c - o.y * s, o.x * s + o.y * c);
			}

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
				o.world = mul(unity_ObjectToWorld, v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				
				o.bUv = rotate(o.uv, _Rotation);

				o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);

				float uvOffset = i.bUv.x * 2 - 1; // -1 -> 1
				float offsetCenter = -(i.world.x - 15) / 12; // 15 is center of grid (-1 -> 1)
				float bright = 0.2 * ((1 - pow(abs(uvOffset - offsetCenter + i.bUv.y * 0.4 * offsetCenter), 0.8)));

				return col * i.color + float4(bright.xxx, 0);
            }
            ENDCG
        }
    }
}
