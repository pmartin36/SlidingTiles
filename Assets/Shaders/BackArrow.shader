Shader "SlidingTiles/BackArrow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

		_InnerTex("Inner Texture", 2D) = "white" {}
		_OuterTex("Outer Texture", 2D) = "white" {}

		_InnerColor("Inner Color", Color) = (1,1,1,1)
		_OuterColor("Outer Color", Color) = (1,1,1,1)
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

            sampler2D _InnerTex;
            sampler2D _OuterTex;
			float4 _InnerColor;
			float4 _OuterColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {		
				float4 tex = tex2D(_MainTex, i.uv);
				float4 color = i.color * lerp(_OuterColor, _InnerColor, tex.r);
				float4 col = lerp(
					tex2D(_OuterTex, i.uv),
					tex2D(_InnerTex, i.uv),
					tex.r
				) * color;
				col.a *= tex.a;
				return col;
            }
            ENDCG
        }
    }
}
