﻿Shader "SlidingTiles/BasicSprite"
{
    Properties
    {
        _Tex ("Main Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
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
			Cull Off
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
				fixed4 color : COLOR0;
            };

            sampler2D _Tex;
            float4 _Tex_ST;
			float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Tex);
				o.color = v.color * _Color;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {		
				fixed4 col = tex2D(_Tex, i.uv) * i.color;
				return col;
            }
            ENDCG
        }
    }
}
