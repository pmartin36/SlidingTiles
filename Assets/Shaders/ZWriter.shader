Shader "SlidingTiles/ZWriter"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { 
			"RenderType"="Opaque" 
			"Queue"="Geometry"
		}

        Pass
        {
			ZWrite On
			ZTest Off

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

            struct v2f { };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {		
				return float4(0,0,0,0);
            }
            ENDCG
        }
    }
	Fallback "Diffuse"
}
