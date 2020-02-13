Shader "SlidingTiles/Tilespace"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		[MaterialToggle] _Sticky("Sticky", Float) = 0
		_StickyAlpha("StickyAlpha", Float) = 1
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
			float _Sticky;
			float _StickyAlpha;

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
                fixed4 col = tex2D(_MainTex, i.uv);
				
				float2 n = (i.uv * 2 - 1);
				float t = _Time.y / 5;
				float c = cos(t);
				float s = sin(t);
				float2 rn = abs(float2(n.x*c - n.y*s, n.x*s + n.y*c));
				float m = max(rn.x, rn.y);
				float len = abs(frac(m + t) - 0.5);

				float square = smoothstep(1, -0.5, len) * smoothstep(1.0, 0.9, max(abs(n.x), abs(n.y)));
				float lerpVal = square * _Sticky;
				return lerp(col * i.color, float4(0.2, 0, 0, _StickyAlpha), lerpVal);
            }
            ENDCG
        }
    }
}
