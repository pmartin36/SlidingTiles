Shader "SlidingTiles/BasicTileTintAndTexSide"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_ImmobileTex("Immobile Texture", 2D) = "white" {}

		_MobileColor("Color", Color) = (1,1,1,1)
		_ImmobileColor("Immobile Color", Color) = (1,1,1,1)

		_Mobile("Mobile", Range(0,1)) = 1

		_Y("Y", float) = 1
		_RightX("Right X", float) = 1
		_LeftX("Left X", float) = 1
    }
    SubShader
    {
        Tags { 
			"RenderType"="Transparent" 
			"Queue"="Transparent-100"
			"DisableBatching"="True"
		}

        Pass
        {
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off

            CGPROGRAM	
            #pragma vertex vert
            #pragma fragment frag
			
            #include "UnityCG.cginc"
			#include "CommonFunctions.cginc"

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
            sampler2D _ImmobileTex;
            float4 _MainTex_ST;
			float4 _MobileColor;
			float4 _ImmobileColor;
			float _Mobile;

			float _Y;
			float _RightX;
			float _LeftX;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = CalculateVertex(v.vertex, _Y, _RightX, _LeftX);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.color = v.color * lerp(_ImmobileColor, _MobileColor, _Mobile);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {		
                fixed4 col = lerp(
					tex2D(_ImmobileTex, i.uv),
					tex2D(_MainTex, i.uv),
					_Mobile
				) * i.color;
				return col;
            }
            ENDCG
        }
    }
}
