Shader "SlidingTiles/Finger"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Radius("Radius", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { 
			"RenderType"="Transparent" 
			"Queue"="Transparent"
		}

		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _Radius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
 
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				
				float max = 0.15;
				float dist = abs(distance(i.uv, float2(0.5, 0.6)) - _Radius);
				dist = 1 - (inverseLerp(0, max, dist) / max);

				float4 ring = lerp(float4(1, 1, 1, 0), float4(1, 1, 1, 1), dist) * (1-step(0.5, col.a));
                return col + ring;
            }
            ENDCG
        }
    }
}
